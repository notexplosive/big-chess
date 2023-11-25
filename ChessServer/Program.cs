using ChessCommon;
using ExplogineCore;
using NetChess;

// load bearing dummy
new SpawnPieceMessage();

var typeLookup = new Dictionary<string, Type>();

foreach (var type in Reflection.GetAllTypesThatDeriveFrom<IClientMessage>())
{
    typeLookup[TypeUtilities.GetTypeName(type)] = type;
    Console.WriteLine(TypeUtilities.GetTypeName(type));
}

var server = new Server("Chess4TheWin", typeLookup);

server.MessageReceived += (sourceId, payload, remoteClients) =>
{
    Console.WriteLine($"Received {payload.GetType().Name} from {sourceId}");

    remoteClients.BroadcastFromClient(sourceId, payload);
};

server.Start(58569);

while (!Console.KeyAvailable)
{
    server.Update();
    Thread.Sleep(15);
}

server.Stop();
