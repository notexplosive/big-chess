using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using Microsoft.Xna.Framework;

namespace BigChess;

public class PulseSquare : AnimatedObject
{
    private readonly Point _position;
    private readonly TweenableFloat _expandAmount = new();
    private readonly TweenableFloat _thickness = new(4);
    private readonly SequenceTween _tween = new();

    public PulseSquare(Point position)
    {
        _position = position;
        _tween
            .Add(
                new MultiplexTween()
                    .AddChannel(_expandAmount.TweenTo(Constants.TileSize / 8f, 0.25f, Ease.CubicFastSlow))
                    .AddChannel(_thickness.TweenTo(0, 0.25f, Ease.Linear))
            )
            .Add(new CallbackTween(() => { Destroy(); }));
    }
    
    public override void DrawScaled(Painter painter)
    {
        var rectangle = Constants.PixelRectangle(_position).Inflated(_expandAmount, _expandAmount);
        painter.DrawLineRectangle(rectangle, new LineDrawSettings {Thickness = _thickness, Color = Color.Goldenrod});
    }

    public override void DrawUnscaled(Painter painter, Matrix canvasToScreen)
    {
        
    }

    public override void Update(float dt)
    {
        _tween.Update(dt);
    }
}
