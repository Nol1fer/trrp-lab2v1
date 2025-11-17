using System.Text.Json;

namespace TVShowsTransfer.Models
{
    public class AppConfig
    {
        public string Mode { get; set; } = "sender";
        public string Transport { get; set; } = "socket";
        public int BatchSize { get; set; } = 50;
        public ConnectionStrings ConnectionStrings { get; set; } = new();
        public SocketConfig SocketConfig { get; set; } = new();
        public RabbitMQConfig RabbitMQConfig { get; set; } = new();

        public static AppConfig LoadFromFile(string filePath = "C:\\Users\\mg\\source\\repos\\trrp-lab2v1\\trrp-lab2v1\\appsettings.json")
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Configuration file not found: {filePath}");

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
    }

    public class ConnectionStrings
    {
        public string SQLite { get; set; } = "";
        public string PostgreSQL { get; set; } = "";
    }

    public class SocketConfig
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 8888;
    }

    public class RabbitMQConfig
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string QueueName { get; set; } = "tvshows_queue";
    }
}