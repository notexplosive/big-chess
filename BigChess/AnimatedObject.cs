using ExplogineMonoGame;
using ExplogineMonoGame.Rails;

namespace BigChess;

public abstract class AnimatedObject : IDrawHook, IUpdateHook
{
    public bool Visible { get; set; } = true;
    public bool ShouldDestroy { get; private set; }

    public void Destroy()
    {
        ShouldDestroy = true;
    }

    public abstract void Draw(Painter painter);
    public abstract void Update(float dt);
}