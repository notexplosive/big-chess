using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace BigChess;

public class ChessBoard
{
    private readonly BoardData _boardData;
    private readonly Dictionary<int, ChessPiece> _pieces = new();

    public ChessBoard(BoardData boardData)
    {
        _boardData = boardData;
    }

    public int IdPool { get; set; }
    public event Action<ChessMove>? PieceMoved;
    public event Action<ChessPiece>? PieceAdded;
    public event Action<ChessPiece>? PiecePromoted;
    public event Action<ChessPiece>? PieceCaptured;
    public event Action<ChessPiece>? PieceDeleted;

    public IEnumerable<int> GetAllIds()
    {
        return _pieces.Keys;
    }
    

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

    public void ForceMovePiece(ChessMove move)
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
            ForceMovePiece(move.NextMove);
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
        return _boardData.IsWithinBoard(position) && GetPieceAt(position) == null;
    }

    public void Promote(int id, PieceType pieceType)
    {
        if (TryFindId(id, out var oldPiece))
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

    public bool TryFindId(int id, out ChessPiece piece)
    {
        return _pieces.TryGetValue(id, out piece);
    }

    public void DeletePiece(int id)
    {
        if (_pieces.ContainsKey(id))
        {
            var oldPiece = _pieces[id];
            _pieces.Remove(id);
            PieceDeleted?.Invoke(oldPiece);
        }
    }

    public SerializedBoard Serialize()
    {
        var result = new SerializedBoard();

        foreach (var piece in _pieces.Values)
        {
            result.Pieces.Add(piece.Serialize());
        }

        return result;
    }

    public void Deserialize(SerializedBoard scenario)
    {
        ClearAllPieces();

        foreach (var piece in scenario.Pieces)
        {
            AddPiece(piece.Deserialize());
        }
    }

    private void ClearAllPieces()
    {
        var ids = _pieces.Values.Select(a => a.Id);

        foreach (var id in ids)
        {
            DeletePiece(id);
        }
    }
}

[Serializable]
public class SerializedBoard
{
    [JsonProperty("pieces")]
    public List<SerializedChessPiece> Pieces { get; set; } = new();
}

[Serializable]
public class SerializedChessPiece
{
    [JsonProperty("position")]
    public SerializedGridPosition Position { get; set; } = new();

    [JsonProperty("type")]
    public PieceType Type { get; set; }
    
    [JsonProperty("color")]
    public PieceColor Color { get; set; }

    public ChessPiece Deserialize()
    {
        return new ChessPiece
        {
            PieceType = Type,
            Color = Color,
            Position = Position.ToPoint()
        };
    }
}