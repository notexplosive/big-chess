using LiteNetLib;
using LiteNetLib.Utils;

namespace NetChess;

public class Client
{
    private readonly NetManager _client;

    public Client(string ip, int targetPort, string connectionKey)
    {
        var listener = new EventBasedNetListener();
        _client = new NetManager(listener);
        _client.Start();
        _client.Connect(ip, targetPort, connectionKey);
        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
        {
            Console.WriteLine($"{dataReader.GetString()}");
            dataReader.Recycle();
        };

        listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Console.WriteLine($"{peer.EndPoint} disconnected by {disconnectInfo.Reason}");
        };
    }

    public void Update()
    {
        _client.PollEvents();
    }

    public void Send(string input)
    {
        var writer = new NetDataWriter();
        writer.Put(input);
        _client.SendToAll(writer, DeliveryMethod.ReliableOrdered);
    }

    public void Stop()
    {
        Console.WriteLine("*** Shutting down...");
        _client.DisconnectAll();
        _client.Stop();
    }
}
