using TVShowsTransfer.Models;
using TVShowsTransfer.Services;
using TVShowsTransfer.Data;

// Load configuration
var config = AppConfig.LoadFromFile();

// Choose transport based on configuration
ITransportService transport = config.Transport.ToLower() switch
{
    "socket" => new SocketTransportService(config.SocketConfig),
    "rabbitmq" => new RabbitMQTransportService(config.RabbitMQConfig),
    _ => throw new ArgumentException($"Unknown transport: {config.Transport}")
};

try
{
    if (config.Mode == "sender")
    {
        await RunSenderMode(config, transport);
    }
    else if (config.Mode == "receiver")
    {
        await RunReceiverMode(config, transport);
    }
    else
    {
        Console.WriteLine($"Unknown mode: {config.Mode}. Use 'sender' or 'receiver'.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Application error: {ex.Message}");
}
finally
{
    await transport.DisconnectAsync();
}

// Sender Mode (PC1 - SQLite)
static async Task RunSenderMode(AppConfig config, ITransportService transport)
{
    Console.WriteLine("Starting in SENDER mode...");

    if (!await transport.ConnectAsync())
        return;

    var reader = new SQLiteReader(config.ConnectionStrings.SQLite);
    var shows = reader.ReadAll();

    Console.WriteLine($"Read {shows.Count} shows from SQLite");

    // Send in batches
    for (int i = 0; i < shows.Count; i += config.BatchSize)
    {
        var batch = shows.Skip(i).Take(config.BatchSize).ToList();

        var message = new Message
        {
            Type = "data",
            Data = System.Text.Json.JsonSerializer.Serialize(batch),
            TotalCount = shows.Count
        };

        await transport.SendAsync(message);
        Console.WriteLine($"Sent batch {i / config.BatchSize + 1}");

        // Small delay to prevent overwhelming
        await Task.Delay(100);
    }

    // Send completion message
    var completeMessage = new Message { Type = "complete", TotalCount = shows.Count };
    await transport.SendAsync(completeMessage);

    Console.WriteLine("All data sent!");
}

// Receiver Mode (PC2 - PostgreSQL)
static async Task RunReceiverMode(AppConfig config, ITransportService transport)
{
    Console.WriteLine("Starting in RECEIVER mode...");

    if (!await transport.ConnectAsync())
        return;

    var writer = new PostgreSQLWriter(config.ConnectionStrings.PostgreSQL);
    var normalizer = new DataNormalizer();
    var receivedCount = 0;

    Console.WriteLine("Waiting for messages...");

    if (transport is RabbitMQTransportService rabbitMqService)
    {
        // RabbitMQ uses event-based consumption
        rabbitMqService.StartConsuming(async message =>
        {
            await ProcessMessage(message, writer, normalizer, receivedCount);
        });

        // Keep running
        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }
    else
    {
        // Socket-based consumption
        while (true)
        {
            var message = await transport.ReceiveAsync();
            if (message == null) break;

            await ProcessMessage(message, writer, normalizer, receivedCount);

            if (message.Type == "complete")
                break;
        }
    }
}

static async Task ProcessMessage(Message message, PostgreSQLWriter writer, DataNormalizer normalizer, int receivedCount)
{
    try
    {
        if (message.Type == "data")
        {
            var batch = System.Text.Json.JsonSerializer.Deserialize<List<DenormalizedTvShow>>(message.Data)!;
            normalizer.NormalizeData(batch);
            receivedCount += batch.Count;
            Console.WriteLine($"Received batch. Total: {receivedCount}");
        }
        else if (message.Type == "complete")
        {
            Console.WriteLine($"All data received! Total: {message.TotalCount} records");
            await writer.SaveNormalizedDataAsync(normalizer);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing message: {ex.Message}");
    }
}