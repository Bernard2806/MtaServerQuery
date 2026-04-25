using MtaServerQuery.Models;

namespace MtaServerQuery.Client;

public interface IMtaServerService
{
    Task<IReadOnlyList<MtaServerResponsePlayer>> GetPlayersAsync(string ip, int port, int timeout = 3000, CancellationToken cancellationToken = default);
    Task<bool> IsConnectedAsync(string playerName, string ip, int port, int timeout = 3000, CancellationToken cancellationToken = default);
}
