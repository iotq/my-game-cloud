using System.Net.WebSockets;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using MyGameCloud.GameServer.Logic;

namespace MyGameCloud.GameServer.Network;

public class WSController(ILogger<WSController> logger, Lobby lobby) : ControllerBase
{

    public static int WebsocketConnectionCount = 0;

    [Route("ws")]
    public async Task Get()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {

            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        if (WebsocketConnectionCount >= 20)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            HttpContext.Response.ContentType = "text/plain; charset=utf-8";
            await HttpContext.Response.WriteAsync("連線數量已達上限，拒絕連線。");
            return;
        }

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        await Listen(webSocket);

    }

    private async Task Listen(WebSocket webSocket)
    {
        Player player = Player.Create(new WSPeer(webSocket), lobby);
        await player.SendLoginInfo();
        lobby.EnterPlayer(player);
        var buffer = new byte[1024 * 4];
        while (webSocket.State == WebSocketState.Open)
        {
            await HandleReceive(webSocket, player, buffer);
        }
    }

    private async Task HandleReceive(WebSocket ws, Player player, byte[] buffer)
    {
        using MemoryStream ms = new();
        WebSocketReceiveResult result;
        try
        {
            do
            {
                result = await ws.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await player.CloseConnection();
                    break;
                }
                ms.Write(buffer, 0, result.Count);

            } while (!result.EndOfMessage);

            if (result.MessageType == WebSocketMessageType.Binary)
            {
                ms.Seek(0, SeekOrigin.Begin);
                Protos.ClientPacket packet = Protos.ClientPacket.Parser.ParseFrom(ms);

                player.ProcessMessage(packet);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed On Parsing: {ex.Message}");
        }

    }

}
