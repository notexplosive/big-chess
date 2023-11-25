using NetChess;
using TestCommon;

namespace TestClient;

public class ConsoleGame
{
    private bool _isRunning = true;

    public void Run()
    {
        var consoleInput = new ConsoleInput();
        var client = new LocalClient("localhost", 9050, "SomeConnectionKey");
        
        new LoadBearingDummy();

        Console.WriteLine($"known types: {string.Join(", ", client.TypeLookup.Keys)}");
        
        client.Disconnected += Quit;

        client.RecievedMessage += OnMessage;

        consoleInput.Submitted += content =>
        {
            if (content.StartsWith("/name "))
            {
                var split = content.Split().ToList();
                split.RemoveAt(0);
                var nameArg = string.Join(' ', split);

                client.SendObject(new RenameRequest {Name = nameArg});
            }
            else
            {
                client.SendObject(new ChatMessageFromClient
                {
                    Content = content
                });
            }
        };
        consoleInput.Finished += Quit;

        var task = new Task(() =>
        {
            while (_isRunning)
            {
                client.Update();
            }
        });

        task.Start();

        while (_isRunning)
        {
            consoleInput.Update();
        }

        client.Stop();
    }

    private void OnMessage(int id, IClientMessage message)
    {
        if (message is ChatMessageFromServer chatMessage)
        {
            Console.WriteLine(chatMessage.ToString());
        }
        else if (message is ConfirmName confirmName)
        {
            Console.WriteLine($"Your name is now {confirmName.Name}");
        }
        else
        {
            Console.WriteLine($"GOT: {id} {message}");
        }
    }

    public void Quit()
    {
        _isRunning = false;
    }
}
