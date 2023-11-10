using System;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Rails;
using ExTween;
using Microsoft.Xna.Framework;

namespace BigChess;

public class OverlayUi : IUpdateHook, IUpdateInputHook, IDrawHook
{
    private readonly IRuntime _runtime;
    private readonly Func<bool> _isEditMode;
    private readonly SequenceTween _tween = new();

    public OverlayUi(ChessGameState gameState, IRuntime runtime, BoardData boardData, Func<bool> isEditMode)
    {
        _runtime = runtime;
        _isEditMode = isEditMode;
        _animatedObjects.Add(new CurrentTurnIndicator(gameState, runtime));
        _animatedObjects.Add(new ActionPointsCounter(gameState, runtime, boardData));

        gameState.TurnChanged += AnnounceTurn;
        gameState.GameEnded += AnnounceGameEnd;
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
        if (!_isEditMode())
        {
            _animatedObjects.Add(new TurnAnnouncement(color, $"{color} to move", _runtime.Window.RenderResolution, true));
        }
    }

    private void AnnounceGameEnd(PieceColor color, bool wasVictory)
    {
        if (!_isEditMode())
        {
            _animatedObjects.Add(new TurnAnnouncement(color, wasVictory ? $"{color} Wins!" : "Stalemate!", _runtime.Window.RenderResolution, false));
        }
    }
}