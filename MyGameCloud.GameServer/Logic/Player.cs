using Google.Protobuf;
using MyGameCloud.GameServer.Network;

namespace MyGameCloud.GameServer.Logic;

public class Player(string id, IPeer peer, Lobby lobby)
{
    public string Id => id;
    public string Name = "";
    public int shortId;
    public IPeer Peer => peer;
    public float X { get; set; }
    public float Y { get; set; }
    public float Rotation { get; set; }
    public int Hp { get; set; }

    public Room? CurrentRoom { get; set; }

    // 儲存上一次成功發送給客戶端的快照
    private PlayerSnapshot? _lastState = null;
    public async Task CloseConnection()
    {
        if (CurrentRoom != null)
        {
            CurrentRoom.ExitPlayer(id);
        }
        else
        {
            lobby.ExitPlayer(id);
        }
        await Peer.Disconnect();
    }

    public static Player Create(IPeer peer, Lobby lobby)
    {
        return new Player(Guid.NewGuid().ToString(), peer, lobby);
    }


    public void ProcessMessage(Protos.ClientPacket packet)
    {
        switch (packet.ContentCase)
        {
            case Protos.ClientPacket.ContentOneofCase.HeartBeat:
                break;

            case Protos.ClientPacket.ContentOneofCase.Move:
                X = packet.Move.X;
                Y = packet.Move.Y;
                break;

            case Protos.ClientPacket.ContentOneofCase.Chat:
                break;
        }
    }

    public async Task SendLoginInfo()
    {
        Protos.ServerPacket packet = new Protos.ServerPacket
        {
            Id = Lobby.PacketIdCursor++,
            Timestamp = DateTime.UtcNow.Ticks,
            Login = new() { PlayerId = Id }
        };

        await peer.Send(packet.ToByteArray());
    }

    public Protos.PlayerContent ToRealtimeProto()
    {
        return new Protos.PlayerContent
        {
            ShortId = shortId,
            X = X,
            Y = Y,
            Rotation = Rotation,
            Hp = Hp
        };
    }

    public Protos.PlayerFullInfo ToFullInfoProto()
    {
        return new Protos.PlayerFullInfo
        {
            Id = Id,
            Name = Name,
            ShortId = shortId,
            X = X,
            Y = Y,
            Rotation = Rotation,
            Hp = Hp
        };
    }

    public bool IsDirty()
    {
        if (_lastState is null) return true;
        return Math.Abs(X - _lastState.X) > 0.01f ||
               Math.Abs(Y - _lastState.Y) > 0.01f ||
               Math.Abs(Rotation - _lastState.Rotation) > 0.1f ||
               Hp != _lastState.Hp;
    }
    public void UpdateSnapshot()
    {
        if (_lastState is null)
        {
            _lastState = new();
        }
        _lastState.X = X;
        _lastState.Y = Y;
        _lastState.Rotation = Rotation;
        _lastState.Hp = Hp;
    }
    private class PlayerSnapshot
    {
        public float X, Y, Rotation;
        public int Hp;
    }

}
