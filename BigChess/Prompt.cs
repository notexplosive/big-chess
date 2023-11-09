using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Rails;
using Microsoft.Xna.Framework;

namespace BigChess;

public abstract class Prompt : IUpdateInputHook, IUpdateHook, IDrawHook
{
    protected readonly HoverState BackgroundHover = new();
    public abstract bool IsOpen { get; }

    public void Draw(Painter painter)
    {
        if (!IsOpen)
        {
            return;
        }

        DrawInternal(painter);
    }

    public abstract void Update(float dt);

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (!IsOpen)
        {
            return;
        }

        var overlayLayer = hitTestStack.AddLayer(Matrix.Identity, Depth.Front + 100);
        overlayLayer.AddInfiniteZone(Depth.Back, BackgroundHover);

        UpdateInputInternal(input, overlayLayer);
    }

    protected abstract void UpdateInputInternal(ConsumableInput input, HitTestStack overlayLayer);

    protected abstract void DrawInternal(Painter painter);
    public abstract void Cancel();
}
