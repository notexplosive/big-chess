using LiteNetLib;

namespace NetChess;

public class RemoteClientCollection
{
    private readonly List<RemoteClient> _content = new();

    public RemoteClient? PeerToClient(NetPeer fromPeer)
    {
        return _content.Find(client => client.Peer == fromPeer);
    }

    public int PeerToId(NetPeer fromPeer)
    {
        var sourceId = -1;
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
            client.SendObject(-1, message);
        }
    }

    public void SendToClientFromServer(int targetId, IClientMessage message)
    {
        var source = FindFromId(targetId);
        source?.SendObject(-1, message);
    }

    public RemoteClient? FindFromId(int clientId)
    {
        return _content.Find(a => a.Id == clientId);
    }

    public void BroadcastFromClient(int originalSenderId, IClientMessage message)
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
