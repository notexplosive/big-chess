using ChessCommon;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using Microsoft.Xna.Framework;

namespace BigChess;

public class CurrentTurnIndicator : AnimatedObject
{
    private readonly SequenceTween _tween = new();
    private readonly TweenableFloat _scale = new(1);
    private readonly TweenableFloat _opacity = new(1);
    private readonly IRuntime _runtime;
    private PieceColor _currentColor;

    public CurrentTurnIndicator(ChessGameState gameState, IRuntime runtime)
    {
        _runtime = runtime;
        gameState.TurnChanged += OnTurnChange;
    }

    private void OnTurnChange(PieceColor color)
    {
        _tween.Clear();

        _tween.Add(
            new MultiplexTween()
                .AddChannel(_scale.TweenTo(1f, 0.25f, Ease.Linear))
                .AddChannel(_opacity.TweenTo(0f, 0.25f, Ease.Linear))
        );

        _tween.Add(new CallbackTween(() =>
        {
            _currentColor = color;
        }));

        _tween.Add(
            new MultiplexTween()
                .AddChannel(_scale.TweenTo(1.003f, 0.25f, Ease.Linear))
                .AddChannel(_opacity.TweenTo(1f, 0.25f, Ease.Linear))
        );
    }

    public override void DrawScaled(Painter painter)
    {
        var color = Constants.PieceColorToRgb(_currentColor);
        var size = _runtime.Window.RenderResolution.ToVector2() / 2f * _scale;
        var rectangle = RectangleF.InflateFrom(size, size.X, size.Y);
        
        painter.DrawLineRectangle(rectangle, new LineDrawSettings{Color = color.WithMultipliedOpacity(_opacity), Depth = Depth.Front, Thickness = 20f});
    }

    public override void DrawUnscaled(Painter painter, Matrix canvasToScreen)
    {
        
    }

    public override void Update(float dt)
    {
        _tween.Update(dt);
    }
}
