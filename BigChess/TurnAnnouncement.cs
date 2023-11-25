using ChessCommon;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using Microsoft.Xna.Framework;

namespace BigChess;

internal class TurnAnnouncement : AnimatedObject
{
    private readonly PieceColor _color;
    private readonly string _text;
    private readonly Point _windowSize;
    private readonly TweenableFloat _barScale = new(1);
    private readonly TweenableFloat _opacity = new(0);
    private readonly TweenableFloat _textScale = new(1);
    private readonly SequenceTween _tween = new();

    public TurnAnnouncement(PieceColor color, string text, Point windowSize, bool hasFadeOut)
    {
        _color = color;
        _text = text;
        _windowSize = windowSize;
        var phase1 = 0.15f;
        var phase2 = 0.25f;
        var phase3 = 1f;
        _tween
            .Add(
                new MultiplexTween()
                    .AddChannel(_barScale.TweenTo(1.2f, phase1, Ease.CubicFastSlow))
                    .AddChannel(_textScale.TweenTo(0.9f, phase1, Ease.CubicFastSlow))
                    .AddChannel(_opacity.TweenTo(1f, phase1 / 2f, Ease.Linear))
            )
            .Add(
                new MultiplexTween()
                    .AddChannel(_barScale.TweenTo(1f, phase2, Ease.CubicFastSlow))
            )
            ;

        if (hasFadeOut)
        {
            _tween
                .Add(new WaitSecondsTween(0.5f))
                .Add(
                    new MultiplexTween()
                        .AddChannel(_opacity.TweenTo(0f, phase3 / 4f, Ease.Linear))
                        .AddChannel(_barScale.TweenTo(2f, phase3, Ease.CubicFastSlow))
                        .AddChannel(_textScale.TweenTo(1.25f, phase3, Ease.CubicFastSlow))
                )
                .Add(new CallbackTween(Destroy))
                ;
        }
    }

    public override void DrawScaled(Painter painter)
    {
        var textColor = Constants.PieceColorToRgb(Constants.OppositeColor(_color)).WithMultipliedOpacity(_opacity);
        var backgroundColor = Constants.PieceColorToRgb(_color).WithMultipliedOpacity(_opacity * 0.75f);

        var windowRectangle = _windowSize.ToRectangleF();
        var barRectangle = RectangleF.FromSizeAlignedWithin(windowRectangle.InflatedMaintainAspectRatio(-100),
            new Vector2(windowRectangle.Width, 140), Alignment.BottomCenter);

        barRectangle = barRectangle.Inflated(barRectangle.Width * _barScale / 4f, barRectangle.Height * _barScale / 4f);
        painter.DrawRectangle(barRectangle, new DrawSettings {Color = backgroundColor, Depth = Depth.Middle});
        painter.DrawStringWithinRectangle(Client.Assets.GetFont("engine/logo-font", (int) (120 * _textScale)),
            _text, barRectangle, Alignment.Center,
            new DrawSettings {Color = textColor, Depth = Depth.Middle - 100});
    }

    public override void DrawUnscaled(Painter painter, Matrix canvasToScreen)
    {
    }

    public override void Update(float dt)
    {
        _tween.Update(dt);
    }
}
