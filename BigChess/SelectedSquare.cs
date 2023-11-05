using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using Microsoft.Xna.Framework;

namespace BigChess;

public class ProposedMoveSquare : AnimatedObject
{
    private Point _position;
    private readonly TweenableFloat _expandAmount = new();
    private readonly TweenableFloat _opacity = new(1);
    private readonly SequenceTween _tween = new();


    public ProposedMoveSquare()
    {
        Visible = false;
        _tween
            .Add(_expandAmount.CallbackSetTo(Constants.TileSize / 8f))
            .Add(_opacity.CallbackSetTo(0))
            .Add(
                new MultiplexTween()
                    .AddChannel(_opacity.TweenTo(0.5f, 0.05f, Ease.Linear))
                    .AddChannel(_expandAmount.TweenTo(0, 0.15f, Ease.CubicFastSlow))
            )
            ;
    }

    public override void DrawScaled(Painter painter)
    {
        
    }

    public override void DrawUnscaled(Painter painter, Camera camera)
    {
        var rectangle = Constants.PixelRectangle(_position).Inflated(_expandAmount, _expandAmount);
        painter.DrawRectangle(RectangleF.Transform(rectangle, camera.CanvasToScreen), new DrawSettings {Depth = Depth.Front + 100, Color = Color.White.WithMultipliedOpacity(_opacity)});
    }

    public override void Update(float dt)
    {
        _tween.Update(dt);
    }

    public void MoveTo(Point position)
    {
        _position = position;
        _tween.Reset();
    }
}

public class SelectedSquare : AnimatedObject
{
    private readonly Point _position;
    private readonly TweenableFloat _expandAmount = new();
    private readonly TweenableFloat _thickness = new(4);
    private readonly TweenableFloat _opacity = new(1);
    private SequenceTween _tween = new();

    public SelectedSquare(Point position)
    {
        _position = position;
        _tween
            .Add(_expandAmount.TweenTo(Constants.TileSize / 8f, 0.25f, Ease.CubicFastSlow))
            .Add(_expandAmount.TweenTo(0, 0.25f, Ease.CubicSlowFast))
            .Add(new WaitSecondsTween(0.25f))
            ;

        _tween.IsLooping = true;
    }

    public override void DrawScaled(Painter painter)
    {
        
    }

    public override void DrawUnscaled(Painter painter, Camera camera)
    {
        var rectangle = Constants.PixelRectangle(_position).Inflated(_expandAmount, _expandAmount);
        painter.DrawLineRectangle(RectangleF.Transform(rectangle, camera.CanvasToScreen), new LineDrawSettings {Depth = Depth.Front + 100, Thickness = _thickness, Color = Color.LightBlue.WithMultipliedOpacity(_opacity)});
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
                    .AddChannel(_opacity.TweenTo(0, 0.15f, Ease.Linear))
            )
            .Add(new CallbackTween(Destroy))
            ;
    }
}
