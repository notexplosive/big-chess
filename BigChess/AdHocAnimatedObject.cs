using System;
using ExplogineMonoGame;

namespace BigChess;

public class AdHocAnimatedObject : AnimatedObject
{
    public event Action<Painter>? OnDraw;
    public event Action<Painter, Camera>? OnDrawUnscaled;
    public event Action<float>? OnUpdate;
    
    public override void DrawScaled(Painter painter)
    {
        OnDraw?.Invoke(painter);
    }

    public override void DrawUnscaled(Painter painter, Camera camera)
    {
        OnDrawUnscaled?.Invoke(painter, camera);
    }

    public override void Update(float dt)
    {
        OnUpdate?.Invoke(dt);
    }
}
