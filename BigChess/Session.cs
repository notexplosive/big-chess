using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using ExplogineMonoGame.Rails;
using Microsoft.Xna.Framework;

namespace BigChess;

public abstract class Session : IUpdateInputHook
{
    public abstract void DragInitiated(Point position);
    public abstract void ClickOn(Point position, MouseButton mouseButton);
    public abstract void DragSucceeded(Point position);
    public abstract void DragFinished(Point? position);
    public abstract void UpdateInput(ConsumableInput input, HitTestStack hitTestStack);
    public abstract void OnExit();
    public abstract void OnEnter();
}