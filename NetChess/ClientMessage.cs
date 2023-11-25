using Newtonsoft.Json;

namespace NetChess;

public interface IClientMessage
{
    
}

[Serializable]
public class ClientMessage<T> : IClientMessage where T : ClientMessage<T>, new()
{
    [JsonProperty("type")]
    public string TypeName { get; set; } = TypeUtilities.GetTypeName<T>();
}