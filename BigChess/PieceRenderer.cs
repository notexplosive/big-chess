using System;
using ChessCommon;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using ExTween;
using ExTweenMonoGame;
using Microsoft.Xna.Framework;

namespace BigChess;

public class PieceRenderer : AnimatedObject
{
    private readonly TweenableFloat _angle = new();
    private readonly Assets _assets;
    private readonly ChessBoard _board;
    private readonly TweenableVector2 _fakePiecePosition = new();
    private readonly TweenableFloat _opacity = new(1f);

    /// <summary>
    ///     Cached info for rendering
    /// </summary>
    private readonly ChessPiece _originalPiece;

    public int PieceId { get; }
    private readonly TweenableFloat _scale = new(1f);
    private bool _isDrawingPieceExactly = true;
    private readonly SequenceTween _isolatedTween = new();
    private bool _isFadingOut;
    private bool _isMoving;

    public PieceRenderer(ChessPiece originalPiece, ChessBoard board, Assets assets)
    {
        _originalPiece = originalPiece;
        PieceId = originalPiece.Id;
        _board = board;
        _assets = assets;
    }

    public override void DrawScaled(Painter painter)
    {
        var frame = Constants.FrameIndex(_originalPiece.PieceType, _originalPiece.Color);
        var depth = Depth.Middle;
        Vector2 position;

        if (!_isDrawingPieceExactly)
        {
            position = _fakePiecePosition;
            depth = Depth.Middle - 1000;
        }
        else
        {
            position = GetPiecePosition();
            _fakePiecePosition.Value = position;
        }

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

    private Vector2 GetPiecePosition()
    {
        var piece = GetPiece();

        if (piece.HasValue)
        {
            var position = Constants.ToWorldPosition(piece.Value.Position);
            return position;
        }

        return _fakePiecePosition;
    }

    public ChessPiece? GetPiece()
    {
        return _board.Pieces.GetPieceFromId(PieceId);
    }

    public override void Update(float dt)
    {
        _isolatedTween.Update(dt);
    }

    public void AnimatePromote()
    {
        _isolatedTween.Add(_scale.TweenTo(1.25f, 0.15f, Ease.QuadFastSlow));
        _isolatedTween.Add(_scale.TweenTo(0.9f, 0.15f, Ease.QuadSlowFast));
        _isolatedTween.Add(_scale.TweenTo(1f, 0.1f, Ease.QuadFastSlow));
    }

    public void AnimateMove(SequenceTween tween, ChessPiece piece, Point newPosition, ChessInput input)
    {
        _isDrawingPieceExactly = false;
        _isMoving = true;
        
        tween.Add(new CallbackTween(()=>
        {
            input.StopInput();
        }));

        var duration =
            ((_fakePiecePosition.Value - Constants.ToWorldPosition(newPosition) +
              new Vector2(Constants.TileSizePixels / 2f)) / Constants.TileSizePixels).Length() / 20f;
        tween.Add(
                new DynamicTween(() =>
                {
                    var result = new MultiplexTween();
                    result.AddChannel(_fakePiecePosition.TweenTo(Constants.ToWorldPosition(newPosition), duration,
                        Ease.Linear));

                    if (piece.PieceType == PieceType.Knight)
                    {
                        result.AddChannel(new SequenceTween()
                            .Add(_scale.TweenTo(1.5f, duration / 2f, Ease.QuadFastSlow))
                            .Add(_scale.TweenTo(0.9f, duration / 2f, Ease.QuadSlowFast))
                            .Add(_scale.TweenTo(1f, duration / 8f, Ease.QuadSlowFast))
                        );
                    }

                    return result;
                }))
            ;
        tween.Add(new CallbackTween(() =>
        {
            _isDrawingPieceExactly = true;
            _isMoving = false;
            input.RestoreInput();
        }));
    }

    public void AnimateDropAt(Point destination)
    {
        _isolatedTween.Add(new CallbackTween(() => { _isDrawingPieceExactly = false; }));

        var duration = 0.15f;
        _isolatedTween.Add(
            new DynamicTween(() =>
            {
                var subTween = new MultiplexTween()
                    .AddChannel(
                        new SequenceTween()
                            .Add(_scale.TweenTo(0.9f, duration, Ease.QuadSlowFast))
                            .Add(_scale.TweenTo(1f, duration, Ease.QuadSlowFast))
                    );

                // If we're already moving we should not mess with the position
                if (!_isMoving)
                {
                    subTween.AddChannel(_fakePiecePosition.TweenTo(Constants.ToWorldPosition(destination), duration / 4f,
                        Ease.Linear));
                }

                return subTween;
            })
        );

        _isolatedTween.Add(new CallbackTween(() => { _isDrawingPieceExactly = true; }));
    }

    public void Drag(Vector2 position)
    {
        _isDrawingPieceExactly = false;
        _fakePiecePosition.Value = position;
        _scale.Value = 1.2f;
    }

    public void FadeOut(SequenceTween tween)
    {
        _isFadingOut = true;
        tween.Add(new CallbackTween(() =>
        {
            _isolatedTween.Add(new CallbackTween(() => { _isDrawingPieceExactly = false; }));

            var duration = 1f;

            var tweenableX = new TweenableFloat(() => _fakePiecePosition.Value.X,
                x => _fakePiecePosition.Value = new Vector2(x, _fakePiecePosition.Value.Y));
            var tweenableY = new TweenableFloat(() => _fakePiecePosition.Value.Y,
                y => _fakePiecePosition.Value = new Vector2(_fakePiecePosition.Value.X, y));

            _isolatedTween.Add(
                new MultiplexTween()
                    .AddChannel(
                        new SequenceTween()
                            .Add(tweenableY.TweenTo(tweenableY.Value - Constants.TileSizePixels * 2, duration / 2,
                                Ease.QuadFastSlow))
                            .Add(tweenableY.TweenTo(tweenableY, duration / 2, Ease.QuadSlowFast))
                    )
                    .AddChannel(tweenableX.TweenTo(
                        tweenableX + Client.Random.Dirty.NextSign() * Client.Random.Dirty.NextFloat(0.25f, 1f) *
                        Constants.TileSizePixels, duration, Ease.Linear))
                    .AddChannel(_angle.TweenTo(MathF.PI * 3, duration, Ease.Linear))
                    .AddChannel(_opacity.TweenTo(0, duration * 0.9f, Ease.Linear))
            );

            _isolatedTween.Add(new CallbackTween(Destroy));
        }));
    }

    public void Delete()
    {
        if (!_isFadingOut)
        {
            Destroy();
        }
    }

    public void SkipTween(SequenceTween tween, ChessInput input)
    {
        tween.SkipToEnd();
        _isolatedTween.SkipToEnd();
    }
}
