using NetChess;
using Newtonsoft.Json;

namespace TestCommon;

public class ChatMessageFromServer : ClientMessage<ChatMessageFromServer>
{
    [JsonProperty("content")]
    public string Content { get; set; } = string.Empty;

    [JsonProperty("sender_name")]
    public string SenderName { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{SenderName}: {Content}";
    }
}

public class ChatMessageFromClient : ClientMessage<ChatMessageFromClient>
{
    [JsonProperty("content")]
    public string Content { get; set; } = string.Empty;
}