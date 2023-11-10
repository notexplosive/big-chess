using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using Microsoft.Xna.Framework;

namespace BigChess;

public class ChessPieceCollection
{
    private Dictionary<int, ChessPiece> Content { get; init; } = new();
    public int IdPool { get; set; }
    public event Action<ChessMove>? PieceMoved;
    public event Action<ChessPiece>? PieceAdded;
    public event Action<ChessPiece>? PieceCaptured;
    public event Action<ChessPiece>? PieceDeleted;

    public IEnumerable<int> GetAllIds()
    {
        return Content.Keys;
    }

    public ChessPiece? GetPieceAt(Point position)
    {
        foreach (var piece in Content.Values)
        {
            if (piece.Position == position)
            {
                return piece;
            }
        }

        return null;
    }

    public void ExecuteMove(ChessMove move)
    {
        var id = move.PieceBeforeMove.Id;
        var currentOccupant = GetPieceAt(move.FinalPosition);
        Content[id] = Content[id] with {Position = move.FinalPosition, HasMoved = true};
        PieceMoved?.Invoke(move);

        if (currentOccupant.HasValue)
        {
            CapturePiece(currentOccupant.Value.Id);
        }

        if (move.NextMove != null)
        {
            ExecuteMove(move.NextMove);
        }
    }

    public ChessPiece AddPiece(ChessPiece pieceTemplate)
    {
        var id = IdPool++;
        Content[id] = pieceTemplate with {Id = id};
        PieceAdded?.Invoke(Content[id]);
        return Content[id];
    }

    public void CapturePiece(int id)
    {
        if (Content.TryGetValue(id, out var piece))
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
        if (Content.TryGetValue(pieceId, out var result))
        {
            return result;
        }

        return null;
    }

    public bool TryFindId(int id, out ChessPiece piece)
    {
        return Content.TryGetValue(id, out piece);
    }

    public void DeletePiece(int id)
    {
        if (Content.ContainsKey(id))
        {
            var oldPiece = Content[id];
            Content.Remove(id);
            PieceDeleted?.Invoke(oldPiece);
        }
    }

    public SerializedBoard Serialize()
    {
        var result = new SerializedBoard();

        foreach (var piece in Content.Values)
        {
            result.Pieces.Add(piece.Serialize());
        }

        return result;
    }

    public void PopulateFromScenario(SerializedScenario scenario)
    {
        ClearAllPieces();

        foreach (var piece in scenario.Board.Pieces)
        {
            AddPiece(piece.Deserialize());
        }
    }

    private void ClearAllPieces()
    {
        var ids = Content.Values.Select(a => a.Id);

        foreach (var id in ids)
        {
            DeletePiece(id);
        }
    }

    public int Count(PieceType type, PieceColor color)
    {
        var count = 0;
        foreach (var piece in Content.Values)
        {
            if (piece.PieceType == type && piece.Color == color)
            {
                count++;
            }
        }

        return count;
    }

    public ChessPieceCollection Clone()
    {
        return new ChessPieceCollection
        {
            Content = new Dictionary<int, ChessPiece>(Content)
        };
    }

    public IEnumerable<ChessPiece> All()
    {
        return Content.Values;
    }
}
