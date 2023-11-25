using LiteNetLib;

namespace NetChess;

public class RemoteClientCollection
{
    private readonly List<RemoteClient> _content = new();

    public RemoteClient? PeerToClient(NetPeer fromPeer)
    {
        return _content.Find(client => client.Peer == fromPeer);
    }

    public RemoteId PeerToId(NetPeer fromPeer)
    {
        var sourceId = RemoteId.Server;
        var sourceClient = PeerToClient(fromPeer);

        if (sourceClient != null)
        {
            sourceId = sourceClient.Id;
        }
        else
        {
            Console.WriteLine($"Unknown client {fromPeer.EndPoint}");
        }

        return sourceId;
    }

    public void Add(RemoteClient remoteClient)
    {
        _content.Add(remoteClient);
    }

    public IEnumerable<RemoteClient> All()
    {
        return _content;
    }

    public void RemoveAllMatchingPeer(NetPeer fromPeer)
    {
        _content.RemoveAll(remoteClient => remoteClient.Peer == fromPeer);
    }

    public void BroadcastFromServer(IClientMessage message)
    {
        foreach (var client in _content)
        {
            client.SendObject(RemoteId.Server, message);
        }
    }

    public void SendToClientFromServer(RemoteId targetId, IClientMessage message)
    {
        var source = FindFromId(targetId);
        source?.SendObject(RemoteId.Server, message);
    }

    public RemoteClient? FindFromId(RemoteId clientId)
    {
        if (clientId.IsServer)
        {
            return null;
        }
        return _content.Find(a => a.Id == clientId);
    }

    public void BroadcastFromClient(RemoteId originalSenderId, IClientMessage message)
    {
        foreach (var client in _content)
        {
            client.SendObject(originalSenderId, message);
        }
    }

    public RemoteClient AddFromPeer(NetPeer fromPeer)
    {
        var newClient = new RemoteClient(fromPeer);
        _content.Add(newClient);
        return newClient;
    }
}
