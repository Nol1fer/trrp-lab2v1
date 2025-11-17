using System.Net.Sockets;
using System.Text;
using TVShowsTransfer.Models;

namespace TVShowsTransfer.Services
{
    public class SocketTransportService : ITransportService
    {
        private readonly SocketConfig _config;
        private TcpClient? _tcpClient;
        private NetworkStream? _stream;

        public bool IsConnected => _tcpClient?.Connected == true;

        public SocketTransportService(SocketConfig config)
        {
            _config = config;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(_config.Host, _config.Port);
                _stream = _tcpClient.GetStream();
                Console.WriteLine($"Connected to {_config.Host}:{_config.Port}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                return false;
            }
        }

        public async Task SendAsync(Message message)
        {
            if (_stream == null) throw new InvalidOperationException("Not connected");

            var json = message.ToJson();
            var bytes = Encoding.UTF8.GetBytes(json);
            var lengthBytes = BitConverter.GetBytes(bytes.Length);

            await _stream.WriteAsync(lengthBytes);
            await _stream.WriteAsync(bytes);
        }

        public async Task<Message?> ReceiveAsync()
        {
            if (_stream == null) return null;

            try
            {
                // Read message length
                var lengthBytes = new byte[4];
                var bytesRead = await _stream.ReadAsync(lengthBytes);
                if (bytesRead == 0) return null;

                var messageLength = BitConverter.ToInt32(lengthBytes);

                // Read message data
                var messageBytes = new byte[messageLength];
                var totalRead = 0;

                while (totalRead < messageLength)
                {
                    var read = await _stream.ReadAsync(messageBytes, totalRead, messageLength - totalRead);
                    if (read == 0) return null;
                    totalRead += read;
                }

                var json = Encoding.UTF8.GetString(messageBytes);
                return Message.FromJson(json);
            }
            catch
            {
                return null;
            }
        }

        public async Task DisconnectAsync()
        {
            _stream?.Close();
            _tcpClient?.Close();
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _tcpClient?.Dispose();
        }
    }
}