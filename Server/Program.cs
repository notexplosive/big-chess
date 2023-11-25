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

var nameLookup = new Dictionary<int, string>();

string LookupSenderName(int id)
{
    return nameLookup.TryGetValue(id, out var value) ? value : "???";
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
            // Actually do the name change
            nameLookup[sourceId] = request.Name;

            // Confirm name change to client
            remoteClients.SendToClientFromServer(sourceId, new ConfirmName
            {
                Name = request.Name
            });
        }
        else if (payload is ChatMessageFromClient outgoingChatMessage)
        {
            // Repackage as a message from the server
            remoteClients.BroadcastFromClient(sourceId,
                new ChatMessageFromServer
                    {Content = outgoingChatMessage.Content, SenderName = LookupSenderName(sourceId)});
        }
        else
        {
            // Rebroadcast
            remoteClients.BroadcastFromClient(sourceId, payload);
        }
    });
