namespace NetChess;

[Serializable]
public class JoinMessage : ClientMessage<JoinMessage>
{
    public RemoteId Id { get; set; }
}