using ExplogineMonoGame;
using ExplogineMonoGame.Rails;

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
    public abstract void DrawUnscaled(Painter painter, Camera camera);
    public abstract void Update(float dt);
}