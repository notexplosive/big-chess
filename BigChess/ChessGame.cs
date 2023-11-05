using System;
using System.Collections.Generic;
using ExplogineMonoGame;
using Microsoft.Xna.Framework;

namespace BigChess;

public class ChessGame
{
    private readonly Dictionary<int, ChessPiece> _pieces = new();

    public int IdPool { get; set; }
    public event Action<ChessPiece, Point, Point>? PieceMoved;
    public event Action<ChessPiece>? PieceAdded;
    public event Action<ChessPiece>? PieceRemoved;

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

    public void MovePiece(int id, Point position)
    {
        var oldPosition = _pieces[id].Position;
        _pieces[id] = _pieces[id] with {Position = position};
        PieceMoved?.Invoke(_pieces[id], oldPosition, position);
    }

    public void AddPiece(ChessPiece pieceTemplate)
    {
        var id = IdPool++;
        _pieces[id] = pieceTemplate with {Id = id};
        PieceAdded?.Invoke(_pieces[id]);
    }

    public void DestroyPiece(int id)
    {
        if (_pieces.ContainsKey(id))
        {
            var piece = _pieces[id];
            _pieces.Remove(id);
            PieceRemoved?.Invoke(piece);
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
}
