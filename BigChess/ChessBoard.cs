using System;
using System.Collections.Generic;
using ExplogineMonoGame;
using Microsoft.Xna.Framework;

namespace BigChess;

public class ChessBoard
{
    private readonly Dictionary<int, ChessPiece> _pieces = new();

    public ChessBoard(ChessGameState gameState)
    {
        PieceMoved += gameState.OnPieceMoved;
        PiecePromoted += gameState.OnPiecePromoted;
    }

    public int IdPool { get; set; }
    public event Action<ChessMove>? PieceMoved;
    public event Action<ChessPiece>? PieceAdded;
    public event Action<ChessPiece>? PiecePromoted;
    public event Action<ChessPiece>? PieceCaptured;
    public event Action<ChessPiece>? PieceDeleted;

    public ChessPiece? GetPieceAt(Point position)
    {
        foreach (var piece in _pieces.Values)
        {
            if (piece.Position == position)
            {
                return piece;
            }
        }

        return null;
    }

    public void MovePiece(ChessMove move)
    {
        var id = move.PieceBeforeMove.Id;
        var currentOccupant = GetPieceAt(move.Position);
        _pieces[id] = _pieces[id] with {Position = move.Position, HasMoved = true};
        PieceMoved?.Invoke(move);

        if (currentOccupant.HasValue)
        {
            CapturePiece(currentOccupant.Value.Id);
        }

        if (move.NextMove != null)
        {
            MovePiece(move.NextMove);
        }
    }

    public ChessPiece AddPiece(ChessPiece pieceTemplate)
    {
        var id = IdPool++;
        _pieces[id] = pieceTemplate with {Id = id};
        PieceAdded?.Invoke(_pieces[id]);
        return _pieces[id];
    }

    public void CapturePiece(int id)
    {
        if (_pieces.TryGetValue(id, out var piece))
        {
            PieceCaptured?.Invoke(piece);
            DeletePiece(id);
        }
        else
        {
            Client.Debug.LogWarning($"Missing id! {id}");
        }
    }

    public ChessPiece? GetPieceFromId(int pieceId)
    {
        if (_pieces.TryGetValue(pieceId, out var result))
        {
            return result;
        }

        return null;
    }

    public bool IsEmptySquare(Point position)
    {
        return Constants.IsWithinBoard(position) && GetPieceAt(position) == null;
    }

    public void Promote(int id, PieceType pieceType)
    {
        if (_pieces.TryGetValue(id, out var oldPiece))
        {
            DeletePiece(id);
            var piece = AddPiece(oldPiece with {PieceType = pieceType});
            PiecePromoted?.Invoke(piece);
        }
        else
        {
            Client.Debug.LogWarning($"Tried to promote {id} but it does not exist");
        }
    }

    private void DeletePiece(int id)
    {
        if (_pieces.ContainsKey(id))
        {
            var oldPiece = _pieces[id];
            _pieces.Remove(id);
            PieceDeleted?.Invoke(oldPiece);
        }
    }
}
