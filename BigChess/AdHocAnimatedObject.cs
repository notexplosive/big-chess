using System;
using ExplogineMonoGame;
using Microsoft.Xna.Framework;

namespace BigChess;

public class AdHocAnimatedObject : AnimatedObject
{
    public event Action<Painter>? OnDraw;
    public event Action<Painter, Matrix>? OnDrawUnscaled;
    public event Action<float>? OnUpdate;
    
    public override void DrawScaled(Painter painter)
    {
        OnDraw?.Invoke(painter);
    }

    public override void DrawUnscaled(Painter painter, Matrix canvasToScreen)
    {
        OnDrawUnscaled?.Invoke(painter, canvasToScreen);
    }

    public override void Update(float dt)
    {
        OnUpdate?.Invoke(dt);
    }
}
