using TVShowsTransfer.Models;

namespace TVShowsTransfer.Services
{
    public interface ITransportService : IDisposable
    {
        Task<bool> ConnectAsync();
        Task DisconnectAsync();
        Task SendAsync(Message message);
        Task<Message?> ReceiveAsync();
        bool IsConnected { get; }
    }
}