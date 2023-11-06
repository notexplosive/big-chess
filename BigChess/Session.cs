using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using ExplogineMonoGame.Rails;
using Microsoft.Xna.Framework;

namespace BigChess;

public abstract class Session
{
    public abstract void DragInitiated(Point position);
    public abstract void ClickOn(Point position, MouseButton mouseButton);
    public abstract void DragSucceeded(Point dragStart, Point position);
    public abstract void DragFinished(Point? position);
}