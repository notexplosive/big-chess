using ExplogineMonoGame;
using ExplogineMonoGame.Rails;

namespace BigChess;

public class DiegeticUi : IDrawHook, IUpdateHook
{
    private SelectedSquare? _selection;
    public AnimatedObjectCollection AnimatedObjects { get; } = new();

    public DiegeticUi(UiState uiState)
    {
        uiState.SelectionChanged += SelectionChanged;
    }

    private void SelectionChanged(ChessPiece? piece)
    {
        // Destroy old selection
        _selection?.FadeOut();

        if (piece != null)
        {
            _selection = AnimatedObjects.Add(new SelectedSquare(piece.Position));
        }
    }

    public void Draw(Painter painter)
    {
        AnimatedObjects.DrawAll(painter);
    }

    public void Update(float dt)
    {
        AnimatedObjects.UpdateAll(dt);
    }
}