using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using ExplogineMonoGame.Rails;
using Microsoft.Xna.Framework;

namespace BigChess;

public abstract class Session
{
    protected abstract void DragInitiated(Point position);
    protected abstract void ClickOn(Point position, MouseButton mouseButton);
    protected abstract void DragSucceeded(Point dragStart, Point position);
    protected abstract void DragFinished(Point? position);
}