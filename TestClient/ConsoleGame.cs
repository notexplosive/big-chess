using NetChess;

namespace TestClient;

public class ConsoleGame
{
    private bool _isRunning = true;

    public void Run()
    {
        var consoleInput = new ConsoleInput();
        var client = new Client("localhost", 9050, "SomeConnectionKey");

        consoleInput.Submitted += input => { client.Send(input); };
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

    public void Quit()
    {
        _isRunning = false;
    }
}
