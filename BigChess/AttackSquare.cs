using System;
using System.ComponentModel;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using Microsoft.Xna.Framework;

namespace BigChess;

public class AttackSquare : TargetSquare
{
    private readonly TweenableFloat _expandAmount = new(Constants.TileSize / 2f);
    private readonly TweenableFloat _opacity = new(1);
    private readonly TweenableFloat _angle = new(0);

    
    public AttackSquare(Point startingPosition, Point landingPosition) : base(landingPosition)
    {
        Tween
            .Add(
                new MultiplexTween()
                    .AddChannel(_expandAmount.TweenTo(-Constants.TileSize / 8f, 0.25f, Ease.CubicSlowFast))
                    .AddChannel(_opacity.TweenTo(0.25f, 0.25f, Ease.Linear))
            )
            .Add(_expandAmount.TweenTo(-Constants.TileSize / 16f, 0.25f, Ease.CubicFastSlow))
            .Add(
                new MultiplexTween()
                    .AddChannel(_opacity.TweenTo(0.85f, 0.35f, Ease.Linear))
                    .AddChannel(
                        new SequenceTween()
                            .Add(_angle.TweenTo(-MathF.PI, 0.25f, Ease.Linear))
                            .Add(_angle.CallbackSetTo(0f))
                        )
            )
            ;

        Tween.IsLooping = true;
    }

    public override void Draw(Painter painter)
    {
        var rectangle = Constants.PixelRectangle(Position).Inflated(_expandAmount, _expandAmount);

        var shapeBounds = rectangle.Inflated(new Vector2(-Constants.TileSize * 0.15f));
        painter.DrawRectangle(shapeBounds.MovedByOrigin(DrawOrigin.Center), new DrawSettings {Color = Color.Red.WithMultipliedOpacity(_opacity), Angle = _angle + MathF.PI/4f, Origin = DrawOrigin.Center});
    }
}
