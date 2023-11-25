using ExplogineCore;

namespace NetChess;

public abstract class ClientMessageTypeOwner
{
    public readonly Dictionary<string, Type> TypeLookup = new();

    protected ClientMessageTypeOwner()
    {
        foreach (var type in Reflection.GetAllTypesThatDeriveFrom<IClientMessage>())
        {
            TypeLookup[TypeUtilities.GetTypeName(type)] = type;
        }
    }
}
