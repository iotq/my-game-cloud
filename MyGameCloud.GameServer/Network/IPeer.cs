namespace MyGameCloud.GameServer.Network;

public interface IPeer
{
    Task Send(byte[] data);

    Task Disconnect();
    bool IsOk();
}
