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
    private readonly ChessBoard _board;
    private readonly ChessGameState _gameState;
    private readonly ChessInput _chessInput;
    private readonly Dictionary<int, PieceRenderer> _pieceRenderers = new();
    private readonly List<TargetSquare> _targetSquares = new();
    private readonly SequenceTween _tween = new();
    private PieceRenderer? _draggedPiece;
    private SelectedSquare? _selection;
    private ProposedMoveSquare _proposedMoveSquare;

    public DiegeticUi(UiState uiState, ChessBoard board, Assets assets, ChessGameState gameState, ChessInput chessInput)
    {
        _proposedMoveSquare = AnimatedObjects.Add(new ProposedMoveSquare());
        
        _board = board;
        _assets = assets;
        _gameState = gameState;
        _chessInput = chessInput;
        uiState.SelectionChanged += SelectionChanged;
        _board.PieceAdded += OnPieceAdded;
        _board.PieceCaptured += OnPieceCaptured;
        _board.PieceDeleted += OnPieceDeleted;
        _board.PieceMoved += OnPieceMoved;
        _board.PiecePromoted += OnPiecePromoted;
        _chessInput.SquareHovered += OnSquareHovered;
        _chessInput.DragFinished += OnDragFinished;
    }

    public AnimatedObjectCollection AnimatedObjects { get; } = new();

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
            _draggedPiece.Drag(input.Mouse.Position(hitTestStack.WorldMatrix) - new Vector2(Constants.TileSize) / 2f);
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
        }
    }

    private void OnPieceMoved(ChessPiece piece, Point previousPosition, Point newPosition)
    {
        if (_pieceRenderers.TryGetValue(piece.Id, out var result))
        {
            result.AnimateMove(_tween, piece, newPosition, _gameState);
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
                if (_board.GetPieceAt(landingPosition) != null)
                {
                    _targetSquares.Add(AnimatedObjects.Add(new AttackSquare(startingPosition, landingPosition)));
                }

                if (Constants.IsWithinBoard(landingPosition))
                {
                    var delay = (startingPosition.ToVector2() - landingPosition.ToVector2()).Length() / 100f;
                    _targetSquares.Add(AnimatedObjects.Add(new MoveSquare(landingPosition, delay)));
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
    
    private void OnDragFinished()
    {
        _proposedMoveSquare.Visible = false;
    }

    public void DropDraggedPiece(Point destination)
    {
        if (_draggedPiece != null)
        {
            _draggedPiece.AnimateDropAt(_tween, destination);
        }

        _draggedPiece = null;
    }

    public void ClearDrag()
    {
        if (_draggedPiece != null)
        {
            var piece = _draggedPiece.GetPiece();
            if (piece.HasValue)
            {
                _draggedPiece.AnimateDropAt(_tween, piece.Value.Position);
            }
        }

        _draggedPiece = null;
    }
}
