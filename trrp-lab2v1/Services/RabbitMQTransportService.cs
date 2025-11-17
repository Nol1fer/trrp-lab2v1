using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using TVShowsTransfer.Models;

namespace TVShowsTransfer.Services
{
    public class RabbitMQTransportService : ITransportService
    {
        private readonly RabbitMQConfig _config;
        private IConnection? _connection;
        private IChannel? _channel;
        private string? _consumerTag;

        public bool IsConnected => _connection?.IsOpen == true;

        public RabbitMQTransportService(RabbitMQConfig config)
        {
            _config = config;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _config.Host,
                    Port = _config.Port,
                    UserName = _config.Username,
                    Password = _config.Password
                };

                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.QueueDeclareAsync(_config.QueueName, durable: false, exclusive: false, autoDelete: false);

                Console.WriteLine($"Connected to RabbitMQ at {_config.Host}:{_config.Port}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RabbitMQ connection failed: {ex.Message}");
                return false;
            }
        }

        public async Task SendAsync(Message message)
        {
            if (_channel == null) throw new InvalidOperationException("Not connected");

            var json = message.ToJson();
            var body = Encoding.UTF8.GetBytes(json);

            await _channel.BasicPublishAsync(exchange: "", routingKey: _config.QueueName, body: body);
        }

        public async Task<Message?> ReceiveAsync()
        {
            throw new NotImplementedException("Use StartConsuming for RabbitMQ");
        }

        public async void StartConsuming(Action<Message> onMessageReceived)
        {
            if (_channel == null) throw new InvalidOperationException("Not connected");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var message = Message.FromJson(json);
                onMessageReceived(message);
                return Task.CompletedTask;
            };

            _consumerTag = await _channel.BasicConsumeAsync(_config.QueueName, autoAck: true, consumer: consumer);
        }

        public async Task DisconnectAsync()
        {
            if (!string.IsNullOrEmpty(_consumerTag))
            {
                await _channel?.BasicCancelAsync(_consumerTag);
            }
            await _channel?.CloseAsync();
            await _connection?.CloseAsync();
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}