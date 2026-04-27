using Google.Protobuf;
using System.Net.WebSockets;

namespace MyGameCloud.GameServer.Network;

public class WSPeer(WebSocket ws): IPeer
{
    public async Task Send(byte[] data)
    {   
        await ws.SendAsync(data, WebSocketMessageType.Binary,true,CancellationToken.None);
    }
    public async Task Disconnect()
    {
        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
    }
    public bool IsOk()
    {
        return ws.State == WebSocketState.Open;
    }
}
