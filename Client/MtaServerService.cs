using System.Net;
using System.Net.Sockets;
using System.Text;
using MtaServerQuery.Models;

namespace MtaServerQuery.Client;

public sealed class MtaServerService : IMtaServerService
{
    private const byte ValidPlayerPrefixMask = 0x3F;

    private IReadOnlyList<MtaServerResponsePlayer>? _cachedPlayers;

    public async Task<IReadOnlyList<MtaServerResponsePlayer>> GetPlayersAsync(string ip, int port, int timeout = 3000, CancellationToken cancellationToken = default)
    {
        var players = new List<MtaServerResponsePlayer>();

        try
        {
            using var udpClient = new UdpClient();
            udpClient.Client.ReceiveTimeout = timeout;
            udpClient.Client.SendTimeout = timeout;

            var endpoint = new IPEndPoint(IPAddress.Parse(ip), port + 123);
            byte[] socketRequestTag = Encoding.ASCII.GetBytes("s");

            await udpClient.Client.ConnectAsync(endpoint);
            await udpClient.SendAsync(socketRequestTag, socketRequestTag.Length);

            using var timeoutCts = new CancellationTokenSource(timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);

            var result = await udpClient.ReceiveAsync(linkedCts.Token);
            var data = result.Buffer;

            if (data is null || data.Length < 10)
            {
                _cachedPlayers = players;
                return players;
            }

            int index = 0;

            if (index + 4 > data.Length || Encoding.ASCII.GetString(data, index, 4) != "EYE1")
            {
                _cachedPlayers = players;
                return players;
            }

            index += 4;

            if (index >= data.Length)
            {
                _cachedPlayers = players;
                return players;
            }

            int length = data[index++];
            if (index + length - 1 > data.Length)
            {
                _cachedPlayers = players;
                return players;
            }

            string gameTag = Encoding.ASCII.GetString(data, index, length - 1);
            if (!string.Equals(gameTag, "mta", StringComparison.Ordinal))
            {
                _cachedPlayers = players;
                return players;
            }

            index += length - 1;

            for (int i = 0; i < 8; i++)
            {
                if (index >= data.Length)
                {
                    _cachedPlayers = players;
                    return players;
                }

                length = data[index++];
                if (index + length - 1 > data.Length)
                {
                    _cachedPlayers = players;
                    return players;
                }

                index += length - 1;
            }

            while (index < data.Length && data[index] != 0x01)
            {
                if (index >= data.Length)
                {
                    break;
                }

                length = data[index++];
                if (index + length - 1 > data.Length)
                {
                    break;
                }

                index += length - 1;

                if (index >= data.Length)
                {
                    break;
                }

                length = data[index++];
                if (index + length - 1 > data.Length)
                {
                    break;
                }

                index += length - 1;
            }

            if (index >= data.Length || data[index] != 0x01)
            {
                _cachedPlayers = players;
                return players;
            }

            index++;

            while (index < data.Length)
            {
                byte prefix = data[index];
                if ((prefix & ValidPlayerPrefixMask) != prefix)
                {
                    break;
                }

                index++;

                if (index >= data.Length)
                {
                    break;
                }

                length = data[index++];
                if (index + length - 1 > data.Length)
                {
                    break;
                }

                string name = Encoding.UTF8.GetString(data, index, length - 1);
                index += length - 1;

                if (index + 2 > data.Length)
                {
                    break;
                }

                index += 2;

                if (index >= data.Length)
                {
                    break;
                }

                length = data[index++];
                if (index + length - 1 > data.Length)
                {
                    break;
                }

                string scoreStr = Encoding.UTF8.GetString(data, index, length - 1);
                index += length - 1;

                if (index >= data.Length)
                {
                    break;
                }

                length = data[index++];
                if (index + length - 1 > data.Length)
                {
                    break;
                }

                string pingStr = Encoding.UTF8.GetString(data, index, length - 1);
                index += length - 1;

                if (index + 1 > data.Length)
                {
                    break;
                }

                index += 1;

                players.Add(new MtaServerResponsePlayer
                {
                    Name = name.TrimEnd('\0'),
                    Score = int.TryParse(scoreStr, out var score) ? score : 0,
                    Ping = int.TryParse(pingStr, out var ping) ? ping : 0
                });
            }
        }
        catch (OperationCanceledException)
        {
            // Ignorado: timeout o cancelación.
        }
        catch (SocketException)
        {
            // Ignorado: error de red al consultar el servidor.
        }

        _cachedPlayers = players;
        return players;
    }

    public async Task<bool> IsConnectedAsync(string playerName, string ip, int port, int timeout = 3000, CancellationToken cancellationToken = default)
    {
        var players = _cachedPlayers;
        if (players is null)
        {
            players = await GetPlayersAsync(ip, port, timeout, cancellationToken);
        }

        if (players.Count == 0)
        {
            return false;
        }

        foreach (var player in players)
        {
            if (string.Equals(player.Name, playerName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
