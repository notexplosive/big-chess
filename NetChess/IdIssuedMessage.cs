namespace NetChess;

[Serializable]
public class IdIssuedMessage : ClientMessage<IdIssuedMessage>
{
    public RemoteId Id { get; set; }
}
