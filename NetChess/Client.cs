using ExplogineCore;

namespace NetChess;

public abstract class Client
{
    public readonly Dictionary<string, Type> TypeLookup = new();

    public Client()
    {
        foreach (var type in Reflection.GetAllTypesThatDeriveFrom<IClientMessage>())
        {
            TypeLookup[TypeUtilities.GetTypeName(type)] = type;
        }
    }
}
