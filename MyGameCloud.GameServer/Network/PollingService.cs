using LiteNetLib;
using LiteNetLib.Utils;
using MyGameCloud.GameServer.Logic;

namespace MyGameCloud.GameServer.Network;

public class PollingService(IConfiguration config, Lobby lobby) : BackgroundService
{
    private NetManager _server;
    private EventBasedNetListener _listener;

    private Dictionary<int, Player> _players = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _listener = new EventBasedNetListener();
        _server = new NetManager(_listener)
        {
            AutoRecycle = true
        };

        _listener.ConnectionRequestEvent += request =>
        {
            if (_server.ConnectedPeersCount < 100)
            {
                request.AcceptIfKey(config["GameKey"]);
            }
            else
            {
                request.Reject();
            }
        };

        _listener.PeerConnectedEvent += async peer =>
        {
            Console.WriteLine($"Client connected: {peer}");
            Player player = Player.Create(new LiteNetPeer(peer, this), lobby);
            await player.SendLoginInfo();
            _players.Add(peer.Id, player);
            lobby.EnterPlayer(player);
        };

        _listener.NetworkReceiveEvent += OnReceive;

        _listener.PeerDisconnectedEvent += (NetPeer peer, DisconnectInfo disconnectInfo) =>
        {
            if (_players.ContainsKey(peer.Id))
            {
                RemovePlayer(peer.Id);
            }

        };

        StartPolling(stoppingToken);
    }

    private void OnReceive(NetPeer peer, NetPacketReader dataReader, byte channel, DeliveryMethod method)
    {
        if (!_players.ContainsKey(peer.Id)) return;
        using MemoryStream stream = new MemoryStream(dataReader.RawData, dataReader.Position, dataReader.AvailableBytes);
        var message = ProtoBuf.Serializer.Deserialize<Protos.ClientPacket>(stream);
        dataReader.Recycle();
        _players[peer.Id].ProcessMessage(message);
    }

    private void StartPolling(CancellationToken stoppingToken)
    {
        _server.Start(9050);
        Console.WriteLine("Server started on port 9050");
        while (!stoppingToken.IsCancellationRequested)
        {
            _server.PollEvents();
            lobby.Boardcast();
            Thread.Sleep(80);
        }
        _server.Stop();
    }

    public void RemovePlayer(int peerId)
    {
        _players.Remove(peerId);
    }

}
