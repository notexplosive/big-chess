namespace NetChess;

public class MessageParse
{
    public static IParseResult ParseResultMessage(string message, Dictionary<string, Type> typeLookup)
    {
        // message format is "<id>: <payload_as_json>"
        var splitColons = message.Split(':').ToList();
        var senderId = int.Parse(splitColons[0]);
        splitColons.RemoveAt(0);

        var payload = string.Join(':', splitColons);

        if (!payload.TryParseJson<ClientMessageFragment>(out var fragment))
        {
            return new FailedParseResult(senderId, "unable to parse", payload);
        }

        var type = typeLookup[fragment.TypeName];
        if (!payload.TryParseJson(type, out var rehydratedPayload))
        {
            return new FailedParseResult(senderId, $"sent a {fragment.TypeName} but it could not be parsed", payload);
        }

        return new SuccessfulParseResult(senderId, (rehydratedPayload as IClientMessage)!, type);
    }

    public interface IParseResult
    {
        int SenderId { get; }
    }

    public class SuccessfulParseResult : IParseResult
    {
        public SuccessfulParseResult(int senderId, IClientMessage payload, Type payloadType)
        {
            SenderId = senderId;
            Payload = payload;
            PayloadType = payloadType;
        }

        public IClientMessage Payload { get; }
        public Type PayloadType { get; }
        public int SenderId { get; }
    }

    public class FailedParseResult : IParseResult
    {
        public FailedParseResult(int senderId, string errorText, string rawPayload)
        {
            SenderId = senderId;
            ErrorText = errorText;
            RawPayload = rawPayload;
        }

        public string ErrorText { get; }
        public string RawPayload { get; }
        public int SenderId { get; }
    }
}
