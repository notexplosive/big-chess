using ExplogineCore;
using NetChess;
using TestCommon;

new LoadBearingDummy();

var typeLookup = new Dictionary<string, Type>();

foreach (var type in Reflection.GetAllTypesThatDeriveFrom<IClientMessage>())
{
    typeLookup[TypeUtilities.GetTypeName(type)] = type;
    Console.WriteLine(TypeUtilities.GetTypeName(type));
}

var nameLookup = new Dictionary<RemoteClient, string>();

string LookupSenderName(RemoteClient? client)
{
    var unknown = "???";
    if (client == null)
    {
        return unknown;
    }

    return nameLookup.TryGetValue(client, out var value) ? value : unknown;
}

Server.Run(9050, "SomeConnectionKey",
    typeLookup,
    (sourceId, payload, remoteClients) =>
    {
        // TODO: this lambda should take a much friendlier "ServerState" object so we don't need to do nonsense like this to find source
        var source = remoteClients.FindFromId(sourceId);

        Console.WriteLine($"Received {payload.GetType().Name} from {sourceId}");
        if (payload is RenameRequest request)
        {
            if (source != null)
            {
                nameLookup[source] = request.Name;
                source.SendObject(-1, new ConfirmName
                {
                    Name = request.Name
                });
            }
        }
        else if (payload is ChatMessageFromClient outgoingChatMessage)
        {
            // Repackage as a message from the server
            remoteClients.BroadcastFromClient(sourceId,
                new ChatMessageFromServer
                    {Content = outgoingChatMessage.Content, SenderName = LookupSenderName(source)});
        }
        else
        {
            // Rebroadcast
            remoteClients.BroadcastFromClient(sourceId, payload);
        }
    });
