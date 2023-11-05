using System.Collections.Generic;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Rails;
using Microsoft.Xna.Framework;

namespace BigChess;

public class DiegeticUi : IDrawHook, IUpdateHook
{
    private readonly Assets _assets;
    private readonly ChessGame _game;
    private readonly Dictionary<int, PieceRenderer> _pieceRenderers = new();
    private SelectedSquare? _selection;

    private readonly List<TargetSquare> _targetSquares = new();

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

    public void Draw(Painter painter)
    {
        AnimatedObjects.DrawAll(painter);
    }

    public void Update(float dt)
    {
        AnimatedObjects.UpdateAll(dt);
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
            _selection = AnimatedObjects.Add(new SelectedSquare(piece.Value.Position));

            foreach (var landingPosition in piece.Value.GetValidMoves(_game))
            {
                if (_game.GetPieceAt(landingPosition) != null)
                {
                    _targetSquares.Add(AnimatedObjects.Add(new AttackSquare(piece.Value.Position, landingPosition)));
                }

                if (Constants.IsWithinBoard(landingPosition))
                {
                    _targetSquares.Add(AnimatedObjects.Add(new MoveSquare(piece.Value.Position, landingPosition)));
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
}
