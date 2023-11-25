namespace TestClient;

public class ConsoleInput
{
    public async void Poll()
    {
        string? input = null;
        while (input?.Trim() != "exit")
        {
            input = await Console.In.ReadLineAsync();

            if (!string.IsNullOrEmpty(input))
            {
                SentData?.Invoke(input!);
            }
        }
        
        Finished?.Invoke();
    }

    public event Action<string>? SentData;

    public event Action? Finished;
}
