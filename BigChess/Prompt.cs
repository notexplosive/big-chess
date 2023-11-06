using System;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Rails;
using Microsoft.Xna.Framework;

namespace BigChess;

public abstract class Prompt: IUpdateInputHook, IUpdateHook, IDrawHook
{
    protected readonly HoverState BackgroundHover = new();
    
    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (!IsActive)
        {
            return;
        }

        var overlayLayer = hitTestStack.AddLayer(Matrix.Identity, Depth.Front + 100);
        overlayLayer.AddInfiniteZone(Depth.Back, BackgroundHover);

        UpdateInputInternal(input, overlayLayer);
    }

    protected abstract void UpdateInputInternal(ConsumableInput input, HitTestStack overlayLayer);
    public abstract void Update(float dt);

    public void Draw(Painter painter)
    {
        if (!IsActive)
        {
            return;
        }

        DrawInternal(painter);
    }

    protected abstract void DrawInternal(Painter painter);
    public abstract bool IsActive { get; }
}
