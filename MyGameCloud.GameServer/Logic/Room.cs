using Google.Protobuf;

namespace MyGameCloud.GameServer.Logic;

public class Room(Lobby lobby, int roomId)
{
    private static int _roomIdCursor = 0;
    public int RoomId => roomId;
    public Dictionary<string, Player> Players = new();
    public long LastUpdateTime { get; set; }

    public void EnterPlayer(Player player)
    {
        lobby.ExitPlayer(player.Id);
        Players.Add(player.Id, player);
        player.X = 1;
        player.Y = 10;
    }

    public void ExitPlayer(string playerId)
    {
        Players.Remove(playerId);
    }


    public void Boardcast()
    {
        Lobby.BoardcastRealtimeToPlayers(Players, ExitPlayer);
    }

    public static Room Create(Lobby lobby)
    {
        return new Room(lobby, _roomIdCursor++);
    }
}
