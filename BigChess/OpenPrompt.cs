using System;
using System.IO;
using ExplogineCore;
using ExplogineMonoGame;
using Newtonsoft.Json;

namespace BigChess;

public class OpenPrompt : ButtonListPrompt
{
    private Action<SerializedBoard>? _bufferedCallback;

    public OpenPrompt(IRuntime runtime) : base(runtime, "Load Scenario")
    {
        
    }

    public override bool IsOpen => _bufferedCallback != null;

    protected override void Cancel()
    {
        _bufferedCallback = null;
    }


    public void Request(Action<SerializedBoard> whenDone)
    {
        _bufferedCallback = whenDone;
        Refresh();
    }

    private void Refresh()
    {
        var files = OpenPrompt.ScenariosFolder.GetFilesAt(".");
        GenerateButtons(files, OpenLevel, file=>new FileInfo(file).Name);
    }

    
    private static IFileSystem ScenariosFolder => Client.Debug.RepoFileSystem.GetDirectory("Scenarios");

    private void OpenLevel(string path)
    {
        var json = ScenariosFolder.ReadFile(path);
        var result = JsonConvert.DeserializeObject<SerializedBoard>(json);

        if (result != null)
        {
            _bufferedCallback?.Invoke(result);
        }
        else
        {
            Client.Debug.LogWarning($"Failed to read {path}");
        }

        _bufferedCallback = null;
    }
}
