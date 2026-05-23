using Google.Protobuf;
using MyGameCloud.GameServer.Logic.Game;
using MyGameCloud.GameServer.Network;
using System.Numerics;

namespace MyGameCloud.GameServer.Logic;

public class Room
{
    private static int _roomIdCursor = 0;
    private Lobby lobby;
    private int roomId;
    public int RoomId => roomId;
    public Dictionary<string, Player> Players = new();
    public long LastUpdateTime { get; set; }

    private int playerIdCursor = 1;

    private int frameSinceLastSnapshotBoardcast = 0;

    private GameWorld world;

    private List<Food> foodToUpdate = new();

    public Room(Lobby lobby, int roomId) {
        this.lobby = lobby;
        this.roomId = roomId;
        world = new(this);
    }

    public void EnterPlayer(Player player)
    {
        lobby.ExitPlayer(player.Id);
        
        player.shortId = playerIdCursor++;
        Random rnd = new Random();
        player.characterId = rnd.Next(4);


        // calcute pos
        float spawnWidth = 750f;
        float spawnHeight = 750f;
        float minDistanceToOtherPlayer = 100f;

        Random rand = new Random();
        for (int trycount = 0; trycount < 50; trycount++)
        {
            float x = rand.NextSingle() * spawnWidth * 2 - spawnWidth;
            float y = rand.NextSingle() * spawnHeight * 2 - spawnHeight;
            
            // 檢查是否重叠
            if(!Players.Values.Any(p =>
            {
                float distance = Vector2.Distance(new(x, y), new(p.X, p.Y));
                double pRadius = Math.Sqrt(p.Mass / Math.PI) * 32;
                return distance < minDistanceToOtherPlayer + pRadius;
            }))
            {
                player.X = x;
                player.Y = y;
                break;
            }
        }
        Players.Add(player.Id, player);
        BoardcaseSnapshot(true);
    }

    public void ExitPlayer(string playerId)
    {
        Players.Remove(playerId);
    }
    public void BoardcaseSnapshot(bool force = false)
    {
        WSController.WebsocketConnectionCount = Players.Count;
        if (!force && frameSinceLastSnapshotBoardcast < 20) return;
        // boardcast the player info to all players
        var packet = CreateRoomSnapshotPacket();
        foreach (var p in Players.Values)
        {
            SendPacketTo(packet, p);
        }
        frameSinceLastSnapshotBoardcast = 0;
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
        packet.Realtime.Foods.AddRange(foodToUpdate.Select(f => f.ToProto()));
        foodToUpdate.Clear();
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
        packet.RoomSnapshot.Foods.AddRange(world.Foods.Values.Select(f => f.ToProto()));
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
        if(packet is null)
        {
            // Clear Disconnected User 
            foreach (var pid in Players.Keys.ToArray())
            {
                var player = Players[pid];
                if (!player.Peer.IsOk())
                {
                    ExitPlayer(player.Id);
                }
            }
            return;
        }
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
        frameSinceLastSnapshotBoardcast = (frameSinceLastSnapshotBoardcast + 1) % 21;
        world.Tick();
        BoardcastRealtimeData();
        BoardcaseSnapshot();
    }

    public static Room Create(Lobby lobby)
    {
        return new Room(lobby, _roomIdCursor++);
    }

    public void OnFoodUpdate(Food food)
    {
        foodToUpdate.Add(food);
    }
}
