using ChessCommon;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using ExTween;
using ExTweenMonoGame;
using Microsoft.Xna.Framework;

namespace BigChess;

public class PromotionFanfare : AnimatedObject
{
    private readonly ChessPiece _piece;
    private readonly TweenableFloat _angle = new(0);
    private readonly Assets _assets;
    private readonly TweenableFloat _opacity = new(0);
    private readonly TweenableVector2 _relativePosition = new();
    private readonly TweenableFloat _scale = new(1f);
    private readonly SequenceTween _tween = new();

    public PromotionFanfare(ChessPiece piece, Assets assets)
    {
        _piece = piece;
        _assets = assets;

        var duration = 0.6f;
        _tween
            .Add(_opacity.CallbackSetTo(0.75f))
            .Add(new MultiplexTween()
                .AddChannel(_scale.TweenTo(5f, duration * 1f, Ease.Linear))
                .AddChannel(_opacity.TweenTo(0f, duration * 0.75f, Ease.Linear))
            )
            .Add(new CallbackTween(Destroy));
    }

    public override void DrawScaled(Painter painter)
    {
        var frame = Constants.FrameIndex(_piece.PieceType, _piece.Color);

        Vector2 position = Constants.ToWorldPosition(_piece.Position)+_relativePosition;
        var depth = Depth.Middle - 2000;

        var rectangle = new RectangleF(position, new Vector2(Constants.TileSizePixels));

        rectangle = rectangle.Inflated(new Vector2(Constants.TileSizePixels) * _scale.Value / 2f -
                                       new Vector2(Constants.TileSizePixels / 2f));

        rectangle = rectangle.MovedByOrigin(DrawOrigin.Center);

        _assets.GetAsset<SpriteSheet>("Pieces").DrawFrameAsRectangle(painter, frame, rectangle,
            new DrawSettings
            {
                Depth = depth, Angle = _angle, Color = Color.White.WithMultipliedOpacity(_opacity),
                Origin = DrawOrigin.Center
            });
    }

    public override void DrawUnscaled(Painter painter, Matrix canvasToScreen)
    {
    }

    public override void Update(float dt)
    {
        _tween.Update(dt);
    }
}
