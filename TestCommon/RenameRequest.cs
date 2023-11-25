using NetChess;

namespace TestCommon;

public class RenameRequest : ClientMessage<RenameRequest>
{
    public string Name { get; set; } = string.Empty;
}
