namespace NetChess;

public readonly struct RemoteId
{
    public bool Equals(RemoteId other)
    {
        return IsServer == other.IsServer && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is RemoteId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IsServer, Id);
    }

    public bool IsServer { get; init; }
    public int Id { get; init; }
    public static RemoteId Server { get; } = new() {IsServer = true, Id = -1};

    public static RemoteId Client(int id)
    {
        return new RemoteId {IsServer = false, Id = id};
    }

    public static bool operator ==(RemoteId left, RemoteId right)
    {
        if (left.IsServer != right.IsServer)
        {
            return false;
        }

        if (left.IsServer && right.IsServer)
        {
            return true;
        }

        return left.Id == right.Id;
    }

    public static bool operator !=(RemoteId left, RemoteId right)
    {
        return !(left == right);
    }

    private const string ServerAsString = "SERVER";

    public static RemoteId Parse(string idAsString)
    {
        if (idAsString == RemoteId.ServerAsString)
        {
            return RemoteId.Server;
        }

        return RemoteId.Client(int.Parse(idAsString));
    }

    public override string ToString()
    {
        if (IsServer)
        {
            return RemoteId.ServerAsString;
        }

        return Id.ToString();
    }
}
