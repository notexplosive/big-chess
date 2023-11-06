using ExplogineMonoGame;

namespace BigChess;

public class EditorPrompt : ButtonListPrompt
{
    private bool _isOpen;
    public override bool IsOpen => _isOpen;

    public void Open()
    {
        _isOpen = true;
        Refresh();
    }

    private void Refresh()
    {
        // GenerateButtons();
    }

    public EditorPrompt(IRuntime runtime) : base(runtime, "Editor")
    {
    }

    protected override void Cancel()
    {
        _isOpen = false;
    }
}
