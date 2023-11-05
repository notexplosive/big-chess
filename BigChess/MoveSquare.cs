using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using Microsoft.Xna.Framework;

namespace BigChess;

public class MoveSquare : TargetSquare
{
    private readonly TweenableFloat _thickness = new(3);
    private readonly TweenableFloat _scale = new(0.9f);
    private readonly TweenableFloat _opacity = new(0);

    public MoveSquare(Point landingPosition, float delay) : base(landingPosition)
    {
        var duration = 0.5f;
        Tween
            .Add(new WaitSecondsTween(delay))
            .Add(
                new MultiplexTween()
                    .AddChannel(
                        new SequenceTween()
                            .Add(_scale.TweenTo(0.4f, duration * 0.75f, Ease.CubicSlowFast))
                            .Add(_scale.TweenTo(0.5f, duration * 0.25f, Ease.CubicFastSlow))
                    )
                    .AddChannel(_opacity.TweenTo(1, duration/2f, Ease.Linear))
            );

    }

    public override void DrawScaled(Painter painter)
    {
    }

    public override void DrawUnscaled(Painter painter, Camera camera)
    {
        var rectangle = RectangleF.InflateFrom(Constants.ToWorldPosition(Position) + new Vector2(Constants.TileSize / 2f), _scale * Constants.TileSize / 2f,
            _scale * Constants.TileSize / 2f);
        var transformedRectangle = RectangleF.Transform(rectangle, camera.CanvasToScreen);

        painter.DrawLineRectangle(transformedRectangle,
            new LineDrawSettings
            {
                Depth = Depth.Back - 100, Thickness = _thickness, Color = Color.Orange.WithMultipliedOpacity(_opacity)
            });
    }
}
