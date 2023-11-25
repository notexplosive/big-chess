using LiteNetLib;
using LiteNetLib.Utils;

namespace TestClient;

public class Client
{
    private readonly NetManager _client;
    private bool _exit;

    public Client()
    {
        var listener = new EventBasedNetListener();
        _client = new NetManager(listener);
        _client.Start();
        _client.Connect("localhost", 9050, "SomeConnectionKey");
        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
        {
            Console.WriteLine("*** We got: {0}", dataReader.GetString(100));
            dataReader.Recycle();
        };
    }

    public void PollLoop()
    {
        while (!_exit)
        {
            _client.PollEvents();
            Thread.Sleep(10);
        }
    }

    public void Send(string input)
    {
        var writer = new NetDataWriter();
        writer.Put(input);
        Console.WriteLine($"*** Sending data: {input}");
        _client.SendToAll(writer, DeliveryMethod.ReliableOrdered);
    }

    public void Stop()
    {
        Console.WriteLine("*** Shutting down...");
        _client.Stop();
        _exit = true;
    }
}
