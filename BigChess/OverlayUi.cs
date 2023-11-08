using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Rails;
using ExTween;
using Microsoft.Xna.Framework;

namespace BigChess;

public class OverlayUi : IUpdateHook, IUpdateInputHook, IDrawHook
{
    private readonly IRuntime _runtime;
    private readonly SequenceTween _tween = new();

    public OverlayUi(ChessGameState gameState, IRuntime runtime, BoardData boardData)
    {
        _runtime = runtime;
        _animatedObjects.Add(new CurrentTurnIndicator(gameState, runtime));

        if (boardData.NumberOfActionPoints > 1)
        {
            _animatedObjects.Add(new ActionPointsCounter(gameState, runtime, boardData));
        }

        gameState.TurnChanged += AnnounceTurn;
    }

    private readonly AnimatedObjectCollection _animatedObjects = new();

    public void Draw(Painter painter)
    {
        _animatedObjects.DrawAll(painter, Matrix.Identity);
    }

    public void Update(float dt)
    {
        _tween.Update(dt);

        if (_tween.IsDone())
        {
            _tween.Clear();
        }

        _animatedObjects.UpdateAll(dt);
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        
    }

    private void AnnounceTurn(PieceColor color)
    {
        _animatedObjects.Add(new TurnAnnouncement(color, _runtime.Window.RenderResolution));
    }
}