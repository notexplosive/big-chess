using ExplogineMonoGame;
using ExplogineMonoGame.Rails;
using Microsoft.Xna.Framework;

namespace BigChess;

public abstract class AnimatedObject : IUpdateHook
{
    public bool Visible { get; set; } = true;
    public bool ShouldDestroy { get; private set; }

    public void Destroy()
    {
        ShouldDestroy = true;
    }

    public abstract void DrawScaled(Painter painter);
    public abstract void DrawUnscaled(Painter painter, Matrix canvasToScreen);
    public abstract void Update(float dt);
}