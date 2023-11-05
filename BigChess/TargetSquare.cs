using ExTween;
using Microsoft.Xna.Framework;

namespace BigChess;

public abstract class TargetSquare : AnimatedObject
{
    protected SequenceTween Tween = new();
    protected Point Position { get; }

    protected TargetSquare(Point position)
    {
        Position = position;
    }

    public override void Update(float dt)
    {
        Tween.Update(dt);
    }
}
