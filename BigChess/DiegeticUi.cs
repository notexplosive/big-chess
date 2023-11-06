using System.Collections.Generic;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Rails;
using ExTween;
using Microsoft.Xna.Framework;

namespace BigChess;

public class DiegeticUi : IUpdateHook, IUpdateInputHook
{
    private readonly Assets _assets;
    private readonly UiState _uiState;
    private readonly ChessBoard _board;
    private readonly ChessInput _chessInput;
    private readonly Dictionary<int, PieceRenderer> _pieceRenderers = new();
    private readonly ProposedMoveSquare _proposedMoveSquare;
    private readonly List<TargetSquare> _targetSquares = new();
    private readonly SequenceTween _tween = new();

    private PieceRenderer? _draggedPiece;
    private SelectedSquare? _selection;

    public DiegeticUi(UiState uiState, ChessBoard board, Assets assets, ChessGameState gameState, ChessInput chessInput)
    {
        _proposedMoveSquare = AnimatedObjects.Add(new ProposedMoveSquare());

        _uiState = uiState;
        _board = board;
        _assets = assets;
        _chessInput = chessInput;
        _uiState.SelectionChanged += SelectionChanged;
        _board.PieceAdded += OnPieceAdded;
        _board.PieceCaptured += OnPieceCaptured;
        _board.PieceDeleted += OnPieceDeleted;
        _board.PieceMoved += OnPieceMoved;
        _board.PiecePromoted += OnPiecePromoted;
        _chessInput.SquareHovered += OnSquareHovered;
        _chessInput.DragFinished += OnDragFinished;
    }

    public AnimatedObjectCollection AnimatedObjects { get; } = new();
    public int? DraggedId => _draggedPiece?.PieceId;

    public void Update(float dt)
    {
        _tween.Update(dt);

        if (_tween.IsDone())
        {
            _tween.Clear();
        }

        AnimatedObjects.UpdateAll(dt);
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (_draggedPiece != null)
        {
            _draggedPiece.Drag(_tween,
                input.Mouse.Position(hitTestStack.WorldMatrix) - new Vector2(Constants.TileSize) / 2f);
        }
    }

    public void Draw(Painter painter, Camera camera)
    {
        AnimatedObjects.DrawAll(painter, camera);

        if (_draggedPiece != null)
        {
            painter.BeginSpriteBatch(camera.CanvasToScreen);
            _draggedPiece.DrawScaled(painter);
            painter.EndSpriteBatch();
        }
    }

    private void OnSquareHovered(Point position)
    {
        if (_draggedPiece != null)
        {
            _proposedMoveSquare.Visible = true;
            _proposedMoveSquare.MoveTo(position);
        }
        else
        {
            _proposedMoveSquare.Visible = false;
        }
    }

    private void OnPiecePromoted(ChessPiece piece)
    {
        if (_pieceRenderers.TryGetValue(piece.Id, out var result))
        {
            result.AnimatePromote();

            AnimatedObjects.Add(new PromotionFanfare(piece, _assets));
        }
    }

    private void OnPieceMoved(ChessMove move)
    {
        if (_pieceRenderers.TryGetValue(move.PieceBeforeMove.Id, out var result))
        {
            result.AnimateMove(_tween, move.PieceBeforeMove, move.Position, _uiState);
        }
    }

    private void OnPieceCaptured(ChessPiece piece)
    {
        if (_pieceRenderers.TryGetValue(piece.Id, out var result))
        {
            result.FadeOut(_tween);
        }
    }

    private void OnPieceDeleted(ChessPiece piece)
    {
        if (_pieceRenderers.TryGetValue(piece.Id, out var result))
        {
            result.Delete();
        }
    }

    private void OnPieceAdded(ChessPiece piece)
    {
        _pieceRenderers.Add(piece.Id, AnimatedObjects.Add(new PieceRenderer(piece, _board, _assets)));
    }

    private void SelectionChanged(ChessPiece? piece)
    {
        ClearOldSelectionState();

        if (piece.HasValue)
        {
            var startingPosition = piece.Value.Position;
            _selection = AnimatedObjects.Add(new SelectedSquare(startingPosition));

            foreach (var landingPosition in piece.Value.GetValidMoves(_board))
            {
                if (_board.GetPieceAt(landingPosition.Position) != null)
                {
                    _targetSquares.Add(
                        AnimatedObjects.Add(new AttackSquare(startingPosition, landingPosition.Position)));
                }

                if (Constants.IsWithinBoard(landingPosition.Position))
                {
                    var delay = (startingPosition.ToVector2() - landingPosition.Position.ToVector2()).Length() / 100f;
                    _targetSquares.Add(AnimatedObjects.Add(new MoveSquare(landingPosition.Position, delay)));
                }
            }
        }
    }

    private void ClearOldSelectionState()
    {
        foreach (var targetSquare in _targetSquares)
        {
            targetSquare.Destroy();
        }

        _targetSquares.Clear();
        _selection?.FadeOut();
    }

    public void BeginDrag(ChessPiece piece)
    {
        _draggedPiece = _pieceRenderers[piece.Id];
    }

    private void OnDragFinished(Point? position)
    {
        _proposedMoveSquare.Visible = false;
    }

    public void ClearDrag()
    {
        // ClearDrag gets called multiple times sometimes, this avoids that problem
        if (_draggedPiece != null)
        {
            var piece = _draggedPiece.GetPiece();
            if (piece.HasValue)
            {
                _draggedPiece.AnimateDropAt(piece.Value.Position);
            }
        }

        _draggedPiece = null;
    }
}
