using LiteNetLib;

namespace NetChess;

public static class Server
{
    public static void Run(int port, string connectionKey, Dictionary<string, Type> typeLookup,
        Action<int, IClientMessage, RemoteClientCollection> onMessage,
        int maxConnections = 10)
    {
        var listener = new EventBasedNetListener();
        var server = new NetManager(listener);
        var remoteClients = new RemoteClientCollection();

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
            reader.Recycle();

            var sourceId = remoteClients.PeerToId(fromPeer);

            var parseResult = MessageParse.ParseResultMessage($"{sourceId}:{content}", typeLookup);

            if (parseResult is MessageParse.SuccessfulParseResult successfulParseResult)
            {
                onMessage(successfulParseResult.SenderId, successfulParseResult.Payload, remoteClients);
            }
            else
            {
                Console.WriteLine($"Failed parse: {content}");
            }
        };

        listener.PeerConnectedEvent += fromPeer =>
        {
            Console.WriteLine($"{fromPeer.EndPoint} connected");
            remoteClients.Add(new RemoteClient(fromPeer));
            remoteClients.BroadcastFromServer(new JoinMessage {Id = fromPeer.Id});
        };

        listener.PeerDisconnectedEvent += (fromPeer, disconnectInfo) =>
        {
            Console.WriteLine($"{fromPeer.EndPoint} disconnected by {disconnectInfo.Reason}");
            remoteClients.RemoveAllMatchingPeer(fromPeer);

            remoteClients.BroadcastFromServer(new LeaveMessage
            {
                Id = fromPeer.Id,
                Reason = disconnectInfo.Reason
            });
        };

        while (!Console.KeyAvailable)
        {
            server.PollEvents();
            Thread.Sleep(15);
        }

        server.Stop();
    }
}
