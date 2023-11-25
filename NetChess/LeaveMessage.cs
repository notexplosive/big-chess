using LiteNetLib;

namespace NetChess;

public class LeaveMessage : ClientMessage<LeaveMessage>
{
    public int Id { get; set; }
    
    public DisconnectReason Reason { get; set; }
}
