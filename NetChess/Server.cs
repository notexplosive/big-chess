using LiteNetLib;
using LiteNetLib.Utils;

namespace NetChess;

public static class Server
{
    public static void Run(int port, string connectionKey, int maxConnections = 10)
    {
        var listener = new EventBasedNetListener();
        var server = new NetManager(listener);
        var peers = new List<NetPeer>();
        
        server.Start(port);
        
        listener.ConnectionRequestEvent += request =>
        {
            if (server.ConnectedPeersCount < maxConnections)
            {
                request.AcceptIfKey(connectionKey);
            }
            else
            {
                request.Reject();
            }
        };
        
        listener.NetworkReceiveEvent += (fromPeer, reader, channel, method) =>
        {
            var content = reader.GetString();
            Console.WriteLine($"{fromPeer.Id}: {content}");
            var writer = new NetDataWriter();
            writer.Put($"{fromPeer.Id}: {content}");
        
            foreach (var peer in peers)
            {
                peer.Send(writer, DeliveryMethod.ReliableOrdered);
            }
        };
        
        listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine($"{peer.EndPoint} connected");
            var writer = new NetDataWriter();
            writer.Put("Hello client!");
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
            peers.Add(peer);
        };
        
        listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Console.WriteLine($"{peer.EndPoint} disconnected by {disconnectInfo.Reason}");
            peers.Remove(peer);
        };
        
        while (!Console.KeyAvailable)
        {
            server.PollEvents();
            Thread.Sleep(15);
        }
        
        server.Stop();

    }
}
