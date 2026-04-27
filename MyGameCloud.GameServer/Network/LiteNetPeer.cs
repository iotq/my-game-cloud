using LiteNetLib;

namespace MyGameCloud.GameServer.Network;

public class LiteNetPeer(NetPeer netPeer, PollingService pollingService): IPeer
{

    public async Task Send(byte[] data)
    {
        netPeer.Send(data, DeliveryMethod.ReliableOrdered);
    }
    public async Task Disconnect()
    {
        pollingService.RemovePlayer(netPeer.Id);
        netPeer.Disconnect();
    }
    public bool IsOk()
    {
        return netPeer.ConnectionState == ConnectionState.Connected;
    }
}
