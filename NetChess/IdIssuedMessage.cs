namespace NetChess;

public class IdIssuedMessage : ClientMessage<IdIssuedMessage>
{
    public int Id { get; set; }
}
