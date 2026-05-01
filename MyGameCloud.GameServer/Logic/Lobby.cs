using Google.Protobuf;

namespace MyGameCloud.GameServer.Logic;

public class Lobby
{
    public static int PacketIdCursor;
    public Dictionary<int, Room> Rooms = new();
    public Dictionary<string, Player> Players = new();

    private bool _autoAllocateRoom = true;
    public void EnterPlayer(Player player)
    {
        if (player.CurrentRoom is not null)
        {
            player.CurrentRoom.ExitPlayer(player.Id);
        }

        if (_autoAllocateRoom)
        {
            AutoAllocateRoom(player);
        }
        else
        {
            Players.Add(player.Id, player);
        }

    }

    public void ExitPlayer(string id)
    {
        Players.Remove(id);
    }

    /// <summary>
    /// 自動分配房間
    /// </summary>
    private void AutoAllocateRoom(Player player)
    {
        Room? targetRoom = Rooms.Values.FirstOrDefault(el => el.Players.Count < 5);
        if (targetRoom is null)
        {
            targetRoom = Room.Create(this);
            Rooms.Add(targetRoom.RoomId, targetRoom);
        }

        targetRoom.EnterPlayer(player);
    }

    public static void BoardcastRealtimeToPlayers(Dictionary<string, Player> playerList, Action<string> removePlayer)
    {

        Protos.ServerPacket packet = new Protos.ServerPacket
        {
            Id = PacketIdCursor++,
            Timestamp = DateTime.UtcNow.Ticks,
            Realtime = new()
        };
        packet.Realtime.Players.AddRange(playerList.Values.Select(p => p.ToRealtimeProto()));
        string[] keys = playerList.Keys.ToArray();
        foreach (var pid in keys)
        {
            var player = playerList[pid];
            if (!player.Peer.IsOk())
            {
                removePlayer(player.Id);
            }
            else
            {
                player.Peer.Send(packet.ToByteArray());
            }
        }
    }

    public void Boardcast()
    {

        foreach (var room in Rooms.Values)
        {
            room.Boardcast();
        }

        if (PacketIdCursor > 1000 * 1000 * 1000)
        {
            PacketIdCursor = 0;
        }
    }
}
