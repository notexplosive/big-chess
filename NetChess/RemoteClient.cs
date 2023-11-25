using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;

namespace NetChess;

public class RemoteClient : Client
{
    public RemoteClient(NetPeer peer)
    {
        Peer = peer;
    }

    public NetPeer Peer { get; }
    public int Id { get; } = RemoteClient.IdPool++;
    
    public static int IdPool = 0;

    private void SendString(int senderId, string content)
    {
        var writer = new NetDataWriter();
        writer.Put($"{senderId}: {content}");
        Peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public void SendObject(int senderId, IClientMessage message)
    {
        Console.WriteLine($"Sending {message.GetType().Name} to {senderId}");
        SendString(senderId, $"{JsonConvert.SerializeObject(message)}");
    }
}
