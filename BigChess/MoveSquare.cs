using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using Microsoft.Xna.Framework;

namespace BigChess;

public class MoveSquare : TargetSquare
{
    private readonly TweenableFloat _expandAmount = new();
    private readonly TweenableFloat _thickness = new(0);
    private readonly TweenableFloat _opacity = new(1);
    
    public MoveSquare(Point startingPosition, Point landingPosition) : base(landingPosition)
    {
        Tween
            .Add(new WaitSecondsTween((startingPosition.ToVector2() - landingPosition.ToVector2()).Length() / 40f))
            .Add(
                new MultiplexTween()
                    .AddChannel(_expandAmount.TweenTo(-Constants.TileSize / 8f, 0.25f, Ease.CubicSlowFast))
                    .AddChannel(_thickness.TweenTo(4, 0.25f, Ease.Linear))
                    .AddChannel(_opacity.TweenTo(1, 0.25f, Ease.Linear))
            )
            .Add(_expandAmount.TweenTo(-Constants.TileSize / 16f, 0.25f, Ease.CubicFastSlow));
    }

    public override void Draw(Painter painter)
    {
        var rectangle = Constants.PixelRectangle(Position).Inflated(_expandAmount, _expandAmount);
        painter.DrawLineRectangle(rectangle, new LineDrawSettings {Depth = Depth.Back - 100, Thickness = _thickness, Color = Color.Orange.WithMultipliedOpacity(_opacity)});
    }
}
