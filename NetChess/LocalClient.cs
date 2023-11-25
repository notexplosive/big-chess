using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;

namespace NetChess;

/// <summary>
/// Client-side representation of the client itself
/// </summary>
public class LocalClient : ClientMessageTypeOwner
{
    private readonly string _ip;
    private readonly int _targetPort;
    private readonly string _connectionKey;
    private readonly NetManager _client;
    private NetPeer _server;

    public event Action<RemoteId, IClientMessage>? ReceivedMessage;
    public event Action? Disconnected;

    public LocalClient(string ip, int targetPort, string connectionKey)
    {
        _ip = ip;
        _targetPort = targetPort;
        _connectionKey = connectionKey;
        var listener = new EventBasedNetListener();
        _client = new NetManager(listener);
        _client.Start();
        _server = _client.Connect(_ip, _targetPort, _connectionKey);

        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
        {
            var message = dataReader.GetString();
            dataReader.Recycle();

            var result = MessageParse.ParseResultMessage(message, TypeLookup);

            if (result is MessageParse.SuccessfulParseResult successfulParseResult)
            {
                ReceivedMessage?.Invoke(result.SenderId,successfulParseResult.Payload);
            }

            if (result is MessageParse.FailedParseResult failedParseResult)
            {
                Console.WriteLine($"{result.SenderId} {failedParseResult}");
            }
        };

        listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Console.WriteLine($"{peer.EndPoint} disconnected by {disconnectInfo.Reason}");
            Disconnected?.Invoke();
        };
    }

    public void Restart()
    {
        Stop();
        
        _client.Start();
        _server = _client.Connect(_ip, _targetPort, _connectionKey);
    }

    public void Update()
    {
        _client.PollEvents();
    }

    public void SendString(string input)
    {
        var writer = new NetDataWriter();
        writer.Put(input);
        _server.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public void Stop()
    {
        Console.WriteLine("*** Shutting down...");
        _client.DisconnectAll();
        _client.Stop();
    }

    public void SendObject<T>(T obj)
    {
        SendString($"{JsonConvert.SerializeObject(obj)}");
    }
}