namespace NetChess;

public class JoinMessage : ClientMessage<JoinMessage>
{
    public int Id { get; set; }
}