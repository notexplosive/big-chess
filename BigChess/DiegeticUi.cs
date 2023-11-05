using System.Collections.Generic;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Rails;
using Microsoft.Xna.Framework;

namespace BigChess;

public class DiegeticUi : IUpdateHook, IUpdateInputHook
{
    private readonly Assets _assets;
    private readonly ChessGame _game;
    private readonly Dictionary<int, PieceRenderer> _pieceRenderers = new();
    private SelectedSquare? _selection;

    private readonly List<TargetSquare> _targetSquares = new();
    private PieceRenderer? _draggedPiece;

    public DiegeticUi(UiState uiState, ChessGame game, Assets assets)
    {
        _game = game;
        _assets = assets;
        uiState.SelectionChanged += SelectionChanged;
        _game.PieceAdded += OnPieceAdded;
        _game.PieceRemoved += OnPieceRemoved;
        _game.PieceMoved += OnPieceMoved;
    }

    public AnimatedObjectCollection AnimatedObjects { get; } = new();

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

    public void Update(float dt)
    {
        AnimatedObjects.UpdateAll(dt);
    }
    
    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (_draggedPiece != null)
        {
            _draggedPiece.Drag(input.Mouse.Position(hitTestStack.WorldMatrix) - new Vector2(Constants.TileSize) / 2f);
        }
    }
    

    private void OnPieceMoved(ChessPiece piece, Point previousPosition, Point newPosition)
    {
        if (_pieceRenderers.TryGetValue(piece.Id, out var result))
        {
            result.AnimateMove(piece, previousPosition, newPosition);
        }
    }

    private void OnPieceRemoved(ChessPiece piece)
    {
        if (_pieceRenderers.TryGetValue(piece.Id, out var result))
        {
            result.Destroy();
        }
    }

    private void OnPieceAdded(ChessPiece piece)
    {
        _pieceRenderers.Add(piece.Id, AnimatedObjects.Add(new PieceRenderer(piece.Id, _game, _assets)));
    }

    private void SelectionChanged(ChessPiece? piece)
    {
        ClearOldSelectionState();

        if (piece.HasValue)
        {
            var startingPosition = piece.Value.Position;
            _selection = AnimatedObjects.Add(new SelectedSquare(startingPosition));

            foreach (var landingPosition in piece.Value.GetValidMoves(_game))
            {
                if (_game.GetPieceAt(landingPosition) != null)
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

    public void DropDraggedPiece(Point destination)
    {
        if (_draggedPiece != null)
        {
            _draggedPiece.AnimateDropAt(destination);
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
                _draggedPiece.AnimateDropAt(piece.Value.Position);
            }
        }
        _draggedPiece = null;
    }
}
