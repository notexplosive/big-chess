using System;
using ExplogineMonoGame;

namespace BigChess;

public class AdHocAnimatedObject : AnimatedObject
{
    public event Action<Painter>? OnDraw;
    public event Action<float>? OnUpdate;
    
    public override void Draw(Painter painter)
    {
        if (Visible)
        {
            OnDraw?.Invoke(painter);
        }
    }

    public override void Update(float dt)
    {
        OnUpdate?.Invoke(dt);
    }
}
