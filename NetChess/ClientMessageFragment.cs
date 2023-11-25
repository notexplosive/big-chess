using Newtonsoft.Json;

namespace NetChess;

public class ClientMessageFragment : ClientMessage<ClientMessageFragment>
{
    
}

public static class TypeUtilities
{
    public static string GetTypeName<T>()
    {
        var type = typeof(T);
        return TypeUtilities.GetTypeName(type);
    }

    public static string GetTypeName(Type type)
    {
        return type.Namespace + "." + type.Name;
    }
    
    public static bool TryParseJson<T>(this string str, out T result)
    {
        var success = TryParseJson(str, typeof(T), out var output);
        result = (T)output;
        return success;
    }

    public static bool TryParseJson(this string str, Type type, out object result)
    {
        bool success = true;
        var settings = new JsonSerializerSettings
        {
            Error = (sender, args) => { success = false; args.ErrorContext.Handled = true; },
            MissingMemberHandling = MissingMemberHandling.Ignore
        };
        result = JsonConvert.DeserializeObject(str, type, settings)!;
        return success;
    }
}
