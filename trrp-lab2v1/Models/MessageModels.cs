using System.Text.Json;

namespace TVShowsTransfer.Models
{
    public class Message
    {
        public string Type { get; set; } = ""; // "data", "complete", "ack"
        public string Data { get; set; } = ""; // JSON data
        public int TotalCount { get; set; }
        public string MessageId { get; set; } = Guid.NewGuid().ToString();

        public string ToJson() => JsonSerializer.Serialize(this);
        public static Message FromJson(string json) => JsonSerializer.Deserialize<Message>(json)!;
    }
}