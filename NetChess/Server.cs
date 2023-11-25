using LiteNetLib;

namespace NetChess;

public class Server
{
    private readonly NetManager _server;

    public Server(string connectionKey, Dictionary<string, Type> typeLookup, int maxConnections = 10)
    {

        var listener = new EventBasedNetListener();
        _server = new NetManager(listener);
        var remoteClients = new RemoteClientCollection();

        listener.ConnectionRequestEvent += request =>
        {
            if (_server.ConnectedPeersCount < maxConnections)
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
                OnMessage(successfulParseResult.SenderId, successfulParseResult.Payload, remoteClients);
            }
            else
            {
                Console.WriteLine($"Failed parse: {content}");
            }
        };

        listener.PeerConnectedEvent += fromPeer =>
        {
            Console.WriteLine($"{fromPeer.EndPoint} connected");

            var client = remoteClients.AddFromPeer(fromPeer);
            client.SendFromServer(new IdIssuedMessage {Id = client.Id});
            remoteClients.BroadcastFromServer(new JoinMessage {Id = client.Id});
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
    }

    private void OnMessage(int senderId, IClientMessage payload, RemoteClientCollection remoteClients)
    {
        MessageReceived?.Invoke(senderId, payload, remoteClients);
    }

    public event Action<int, IClientMessage, RemoteClientCollection>? MessageReceived;

    public void Start(int port)
    {
        _server.Start(port);
    }

    public void Update()
    {
        _server.PollEvents();
    }

    public void Stop()
    {
        _server.Stop();
    }
}
