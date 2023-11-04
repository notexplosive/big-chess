using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BigChess;

public class ChessGame
{
    public List<ChessPiece> Pieces { get; } = new();

    public ChessPiece? GetPieceAt(Point position)
    {
        foreach (var piece in Pieces)
        {
            if (piece.Position == position)
            {
                return piece;
            }
        }

        return null;
    }
}