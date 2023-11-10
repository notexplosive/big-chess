using System.Collections.Generic;
using System.Diagnostics;
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
    private readonly BoardData _boardData;
    private readonly Dictionary<int, PieceRenderer> _pieceRenderers = new();
    private readonly ProposedMoveSquare _proposedMoveSquare;
    private readonly List<TargetSquare> _targetSquares = new();
    private readonly SequenceTween _tween = new();

    private PieceRenderer? _draggedPiece;
    private SelectedSquare? _selection;

    public DiegeticUi(UiState uiState, ChessBoard board, Assets assets, ChessInput chessInput, BoardData boardData)
    {
        _proposedMoveSquare = _animatedObjects.Add(new ProposedMoveSquare());

        _uiState = uiState;
        _board = board;
        _assets = assets;
        _chessInput = chessInput;
        _boardData = boardData;
        _uiState.SelectionChanged += SelectionChanged;
        _board.Pieces.PieceAdded += OnPieceAdded;
        _board.Pieces.PieceCaptured += OnPieceCaptured;
        _board.Pieces.PieceDeleted += OnPieceDeleted;
        _board.Pieces.PieceMoved += OnPieceMoved;
        _board.PiecePromoted += OnPiecePromoted;
        _chessInput.SquareHovered += OnSquareHovered;
        _chessInput.DragFinished += OnDragFinished;
    }

    private readonly AnimatedObjectCollection _animatedObjects = new();
    public int? DraggedId => _draggedPiece?.PieceId;

    public void Update(float dt)
    {
        _tween.Update(dt);

        if (_tween.IsDone())
        {
            _tween.Clear();
        }

        _animatedObjects.UpdateAll(dt);
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (_draggedPiece != null)
        {
            _draggedPiece.Drag(input.Mouse.Position(hitTestStack.WorldMatrix) - new Vector2(Constants.TileSizePixels) / 2f);
        }
    }

    public void Draw(Painter painter, Matrix canvasToScreen)
    {
        _animatedObjects.DrawAll(painter, canvasToScreen);

        if (_draggedPiece != null)
        {
            painter.BeginSpriteBatch(canvasToScreen);
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

            _animatedObjects.Add(new PromotionFanfare(piece, _assets));
        }
    }

    private void OnPieceMoved(ChessMove move)
    {
        if (_pieceRenderers.TryGetValue(move.PieceBeforeMove.Id, out var result))
        {
            result.AnimateMove(_tween, move.PieceBeforeMove, move.FinalPosition, _chessInput);
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
        _pieceRenderers.Add(piece.Id, _animatedObjects.Add(new PieceRenderer(piece, _board, _assets)));
    }

    private void SelectionChanged(ChessPiece? piece)
    {
        ClearOldSelectionState();

        if (piece.HasValue)
        {
            var startingPosition = piece.Value.Position;
            _selection = _animatedObjects.Add(new SelectedSquare(startingPosition));

            foreach (var landingPosition in piece.Value.GetPermittedMoves(_board))
            {
                if (_board.Pieces.GetPieceAt(landingPosition.FinalPosition) != null)
                {
                    _targetSquares.Add(
                        _animatedObjects.Add(new AttackSquare(startingPosition, landingPosition.FinalPosition)));
                }

                if (_boardData.IsWithinBoard(landingPosition.FinalPosition))
                {
                    var delay = (startingPosition.ToVector2() - landingPosition.FinalPosition.ToVector2()).Length() / 100f;
                    _targetSquares.Add(_animatedObjects.Add(new MoveSquare(landingPosition.FinalPosition, delay)));
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
        _draggedPiece.SkipTween(_tween, _chessInput);
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
