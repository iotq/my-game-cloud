using Google.Protobuf;

namespace MyGameCloud.GameServer.Logic;

public class Room(Lobby lobby, int roomId)
{
    private static int _roomIdCursor = 0;
    public int RoomId => roomId;
    public Dictionary<string, Player> Players = new();
    public long LastUpdateTime { get; set; }

    private int playerIdCursor = 1;

    public void EnterPlayer(Player player)
    {
        lobby.ExitPlayer(player.Id);
        Players.Add(player.Id, player);
        player.X = 1;
        player.Y = 10;
        player.shortId = playerIdCursor++;
        Random rnd = new Random();
        player.characterId = rnd.Next(4);

        // boardcast the player info to all players
        var packet = CreateRoomSnapshotPacket();
        foreach(var p in Players.Values)
        {
            SendPacketTo(packet, p);
        }
    }

    public void ExitPlayer(string playerId)
    {
        Players.Remove(playerId);
    }

    private Protos.ServerPacket? GetRealtimePacket()
    {
        List<Player> dirtyPlayers = new List<Player>();
        foreach (var player in Players.Values)
        {
            if (player.IsDirty())
            {
                dirtyPlayers.Add(player);
                player.UpdateSnapshot();
            }
        }

        if (dirtyPlayers.Count == 0) return null;
        Protos.ServerPacket packet = new Protos.ServerPacket
        {
            Id = Lobby.PacketIdCursor++,
            Timestamp = DateTime.UtcNow.Ticks,
            Realtime = new()
        };
        packet.Realtime.Players.AddRange(dirtyPlayers.Select(p => p.ToRealtimeProto()));
        return packet;
    }
    private Protos.ServerPacket CreateRoomSnapshotPacket()
    {
        Protos.ServerPacket packet = new Protos.ServerPacket
        {
            Id = Lobby.PacketIdCursor++,
            Timestamp = DateTime.UtcNow.Ticks,
            RoomSnapshot = new()
        };
        packet.RoomSnapshot.Players.AddRange(Players.Values.Select(p => p.ToFullInfoProto()));
        return packet;
    }
    private void SendPacketTo(Protos.ServerPacket packet, Player target)
    {
        if (!target.Peer.IsOk()) return;
        target.Peer.Send(packet.ToByteArray());
    }
    private void BoardcastRealtimeData()
    {

        Protos.ServerPacket? packet = GetRealtimePacket();
        if(packet is null) return;
        string[] keys = Players.Keys.ToArray();
        foreach (var pid in keys)
        {
            var player = Players[pid];
            if (!player.Peer.IsOk())
            {
                ExitPlayer(player.Id);
            }
            else
            {
                player.Peer.Send(packet.ToByteArray());
            }
        }

    }
    public void Boardcast()
    {
        BoardcastRealtimeData();
    }

    public static Room Create(Lobby lobby)
    {
        return new Room(lobby, _roomIdCursor++);
    }
}
