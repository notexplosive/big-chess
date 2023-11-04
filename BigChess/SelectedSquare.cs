using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using Microsoft.Xna.Framework;

namespace BigChess;

public class SelectedSquare : AnimatedObject
{
    private readonly TweenableFloat _expandAmount = new();
    private readonly Point _position;
    private readonly TweenableFloat _thickness = new(4);
    private readonly TweenableFloat _opacity = new(1);
    private SequenceTween _tween = new();

    public SelectedSquare(Point position)
    {
        _position = position;
        _tween
            .Add(_expandAmount.TweenTo(Constants.TileSize / 8f, 0.25f, Ease.CubicSlowFast))
            .Add(_expandAmount.TweenTo(0, 0.25f, Ease.CubicFastSlow));

        _tween.IsLooping = true;
    }

    public override void Draw(Painter painter)
    {
        var rectangle = Constants.PixelRectangle(_position).Inflated(_expandAmount, _expandAmount);
        painter.DrawLineRectangle(rectangle, new LineDrawSettings {Thickness = _thickness, Color = Color.Goldenrod.WithMultipliedOpacity(_opacity)});
    }

    public override void Update(float dt)
    {
        _tween.Update(dt);
    }

    public void FadeOut()
    {
        _tween = new SequenceTween()
            .Add(
                new MultiplexTween()
                    .AddChannel(_expandAmount.TweenTo(Constants.TileSize / 4f, 0.5f, Ease.CubicFastSlow))
                    .AddChannel(_opacity.TweenTo(0, 0.25f, Ease.Linear))
            )
            .Add(new CallbackTween(Destroy))
            ;
    }
}
