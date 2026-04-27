using Google.Protobuf;
using MyGameCloud.GameServer.Network;

namespace MyGameCloud.GameServer.Logic;

public class Player(string id, IPeer peer, Lobby lobby)
{
    public string Id => id;
    public IPeer Peer => peer;
    public float X { get; set; }
    public float Y { get; set; }
    public float Rotation { get; set; }
    public int Hp { get; set; }

    public Room? CurrentRoom { get; set; }

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

    public Protos.PlayerContent ToProto()
    {
        return new Protos.PlayerContent
        {
            Id = Id,
            X = X,
            Y = Y,
            Rotation = Rotation,
            Hp = Hp
        };
    }
}
