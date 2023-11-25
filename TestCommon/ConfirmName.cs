using NetChess;

namespace TestCommon;

public class ConfirmName : ClientMessage<ConfirmName>
{
    public string Name { get; set; } = String.Empty;
}
