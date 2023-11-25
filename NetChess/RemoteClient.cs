using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;

namespace NetChess;


/// <summary>
/// Serverside representation of a client
/// </summary>
public class RemoteClient : ClientMessageTypeOwner
{
    private static int IdPool;

    public RemoteClient(NetPeer peer)
    {
        Peer = peer;
    }

    public NetPeer Peer { get; }
    public RemoteId Id { get; } = RemoteId.Client(RemoteClient.IdPool++);

    private void SendString(RemoteId senderId, string content)
    {
        var writer = new NetDataWriter();
        writer.Put($"{senderId}: {content}");
        Peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public void SendObject(RemoteId senderId, IClientMessage message)
    {
        Console.WriteLine($"Sending {message.GetType().Name} to {Id}");
        SendString(senderId, $"{JsonConvert.SerializeObject(message)}");
    }

    public void SendFromServer(IClientMessage message)
    {
        SendObject(RemoteId.Server, message);
    }
}
