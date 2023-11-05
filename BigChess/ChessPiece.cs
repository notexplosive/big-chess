using System.Collections.Generic;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace BigChess;

public readonly record struct ChessPiece
{
    public int Id { get; init; }
    public Point Position { get; init; }
    public PieceType PieceType { get; init; }
    public PieceColor Color { get; init; }

    public List<Point> GetValidMoves(ChessGame game)
    {
        var result = new List<Point>();

        if (PieceType == PieceType.Knight)
        {
            AddIfEmptyOrEnemy(Position + new Point(1, 2), game, result);
            AddIfEmptyOrEnemy(Position + new Point(1, -2), game, result);
            AddIfEmptyOrEnemy(Position + new Point(2, 1), game, result);
            AddIfEmptyOrEnemy(Position + new Point(2, -1), game, result);
            AddIfEmptyOrEnemy(Position + new Point(-2, 1), game, result);
            AddIfEmptyOrEnemy(Position + new Point(-2, -1), game, result);
            AddIfEmptyOrEnemy(Position + new Point(-1, -2), game, result);
            AddIfEmptyOrEnemy(Position + new Point(-1, 2), game, result);
        }
        else if (PieceType == PieceType.Bishop)
        {
            Project(game, new Point(1, 1), result);
            Project(game, new Point(1, -1), result);
            Project(game, new Point(-1, 1), result);
            Project(game, new Point(-1, -1), result);
        }
        else if (PieceType == PieceType.Rook)
        {
            Project(game, new Point(0, 1), result);
            Project(game, new Point(0, -1), result);
            Project(game, new Point(-1, 0), result);
            Project(game, new Point(1, 0), result);
        }

        return result;
    }

    private void Project(ChessGame game, Point step, List<Point> result)
    {
        var currentSquare = Position + step;
        while (game.IsEmptySquare(currentSquare))
        {
            result.Add(currentSquare);
            currentSquare += step;
        }

        var piece = game.GetPieceAt(currentSquare);
        if (piece.HasValue && piece.Value.Color != Color)
        {
            // enemy there, add it!
            result.Add(currentSquare);
        }
    }

    private void AddIfEmptyOrEnemy(Point target, ChessGame game, List<Point> result)
    {
        var piece = game.GetPieceAt(target);
        if (game.IsEmptySquare(target) || (piece.HasValue && piece.Value.Color != Color))
        {
            result.Add(target);
        }
        
    }

    public override string ToString()
    {
        return $"[{Id}] {Color} {PieceType} at ({Position.X},{Position.Y})";
    }
}