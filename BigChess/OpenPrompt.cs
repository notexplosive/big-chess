using System;
using System.Collections.Generic;
using System.IO;
using ExplogineCore;
using ExplogineMonoGame;
using Newtonsoft.Json;

namespace BigChess;

public class OpenPrompt : ButtonListPrompt
{
    private Action<SerializedScenario>? _bufferedCallback;

    public OpenPrompt(IRuntime runtime) : base(runtime, "Load Scenario")
    {
        
    }

    public override bool IsOpen => _bufferedCallback != null;

    public override void Cancel()
    {
        _bufferedCallback = null;
    }


    public void Request(Action<SerializedScenario> whenDone)
    {
        _bufferedCallback = whenDone;
        Refresh();
    }

    private void Refresh()
    {
        var files = OpenPrompt.ScenariosFolder.GetFilesAt(".");

        var buttonTemplates = new List<ButtonTemplate>();

        foreach (var file in files)
        {
            buttonTemplates.Add(new ButtonTemplate(new FileInfo(file).Name, ()=> OpenLevel(file)));
        }

        GenerateButtons(buttonTemplates);
    }

    
    private static IFileSystem ScenariosFolder => Client.Debug.RepoFileSystem.GetDirectory("Scenarios");

    private void OpenLevel(string path)
    {
        var json = ScenariosFolder.ReadFile(path);
        var result = JsonConvert.DeserializeObject<SerializedScenario>(json);

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
