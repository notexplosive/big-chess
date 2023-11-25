using LiteNetLib;
using LiteNetLib.Utils;

var listener = new EventBasedNetListener();
var server = new NetManager(listener);
server.Start(9050 /* port */);

listener.ConnectionRequestEvent += request =>
{
    if (server.ConnectedPeersCount < 10 /* max connections */)
    {
        request.AcceptIfKey("SomeConnectionKey");
    }
    else
    {
        request.Reject();
    }
};

listener.NetworkReceiveEvent += (peer, reader, channel, method) =>
{
    Console.WriteLine($"{peer.Id}: {reader.GetString()}");
};

listener.PeerConnectedEvent += peer =>
{
    Console.WriteLine("We got connection: {0}", peer.EndPoint); // Show peer ip
    var writer = new NetDataWriter(); // Create writer class
    writer.Put("Hello client!"); // Put some string
    peer.Send(writer, DeliveryMethod.ReliableOrdered); // Send with reliability
};

while (!Console.KeyAvailable)
{
    server.PollEvents();
    Thread.Sleep(15);
}

server.Stop();
