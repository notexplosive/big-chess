using System.Diagnostics.CodeAnalysis;
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
    private readonly Assets _assets;
    private readonly TweenableVector2 _fakePiecePosition = new();
    private readonly TweenableFloat _scale = new(1f);
    private readonly ChessGame _game;
    private readonly int _pieceId;
    private readonly SequenceTween _tween = new();
    private bool _isDrawingPieceExactly = true;

    public PieceRenderer(int pieceId, ChessGame game, Assets assets)
    {
        _pieceId = pieceId;
        _game = game;
        _assets = assets;
    }

    public override void Draw(Painter painter)
    {
        var piece = GetPiece();

        if (piece.HasValue)
        {
            var frame = Constants.FrameIndex(piece.Value.PieceType, piece.Value.Color);
            var position = GetPiecePosition();
            var depth = Depth.Middle;
            
            if (!_isDrawingPieceExactly)
            {
                position = _fakePiecePosition;
                depth = Depth.Middle - 1000;
            }

            var rectangle = RectangleF.InflateFrom(position + new Vector2(Constants.TileSize / 2f), Constants.TileSize * _scale / 2f, Constants.TileSize * _scale / 2f);
            _assets.GetAsset<SpriteSheet>("Pieces").DrawFrameAsRectangle(painter, frame, rectangle,
                new DrawSettings {Depth = depth});
        }
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

    private ChessPiece? GetPiece()
    {
        return _game.GetPieceFromId(_pieceId);
    }

    public override void Update(float dt)
    {
        _tween.Update(dt);

        if (_tween.IsDone())
        {
            var piece = GetPiece();
            if (piece.HasValue)
            {
                _fakePiecePosition.Value = GetPiecePosition();
            }

            _tween.Clear();
        }
    }

    public void AnimateMove(ChessPiece piece, Point previousPosition, Point newPosition)
    {
        _tween.Add(new CallbackTween(() => { _isDrawingPieceExactly = false; }));

        var duration = (previousPosition.ToVector2() - newPosition.ToVector2()).Length() / 20f;
        _tween.Add(
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
        _tween.Add(new CallbackTween(() => { _isDrawingPieceExactly = true; }));
    }
}
