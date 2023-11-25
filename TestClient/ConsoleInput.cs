namespace TestClient;

public class ConsoleInput
{
    public void Update()
    {
        string? input = null;
        input = Console.ReadLine();

        if (!string.IsNullOrEmpty(input))
        {
            Submitted?.Invoke(input!);
        }

        if (input == "exit")
        {
            Finished?.Invoke();
        }
    }

    public event Action<string>? Submitted;

    public event Action? Finished;
}
