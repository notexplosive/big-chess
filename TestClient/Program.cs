using TestClient;

var consoleInput = new ConsoleInput();
var client = new Client();

consoleInput.SentData += input => { client.Send(input); };

consoleInput.Finished += () => { client.Stop(); };

consoleInput.Poll();
client.PollLoop();
