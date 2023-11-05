using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BigChess;

public readonly record struct ChessPiece
{
    public int Id { get; init; }
    public Point Position { get; init; }
    public PieceType PieceType { get; init; }
    public bool HasMoved { get; init; }
    public PieceColor Color { get; init; }

    public List<Point> GetValidMoves(ChessBoard board)
    {
        var result = new List<Point>();

        if (PieceType == PieceType.Knight)
        {
            AddIfEmptyOrEnemy(Position + new Point(1, 2), board, result);
            AddIfEmptyOrEnemy(Position + new Point(1, -2), board, result);
            AddIfEmptyOrEnemy(Position + new Point(2, 1), board, result);
            AddIfEmptyOrEnemy(Position + new Point(2, -1), board, result);
            AddIfEmptyOrEnemy(Position + new Point(-2, 1), board, result);
            AddIfEmptyOrEnemy(Position + new Point(-2, -1), board, result);
            AddIfEmptyOrEnemy(Position + new Point(-1, -2), board, result);
            AddIfEmptyOrEnemy(Position + new Point(-1, 2), board, result);
        }
        else if (PieceType == PieceType.Bishop)
        {
            Project(board, new Point(1, 1), result);
            Project(board, new Point(1, -1), result);
            Project(board, new Point(-1, 1), result);
            Project(board, new Point(-1, -1), result);
        }
        else if (PieceType == PieceType.Rook)
        {
            Project(board, new Point(0, 1), result);
            Project(board, new Point(0, -1), result);
            Project(board, new Point(-1, 0), result);
            Project(board, new Point(1, 0), result);
        }
        else if (PieceType == PieceType.Queen)
        {
            Project(board, new Point(0, 1), result);
            Project(board, new Point(0, -1), result);
            Project(board, new Point(-1, 0), result);
            Project(board, new Point(1, 0), result);
            Project(board, new Point(1, 1), result);
            Project(board, new Point(1, -1), result);
            Project(board, new Point(-1, 1), result);
            Project(board, new Point(-1, -1), result);
        }
        else if (PieceType == PieceType.King)
        {
            // King is allowed to move even if it puts itself in check
            AddIfEmptyOrEnemy(Position + new Point(0, 1), board, result);
            AddIfEmptyOrEnemy(Position + new Point(0, -1), board, result);
            AddIfEmptyOrEnemy(Position + new Point(-1, 0), board, result);
            AddIfEmptyOrEnemy(Position + new Point(1, 0), board, result);
            AddIfEmptyOrEnemy(Position + new Point(1, 1), board, result);
            AddIfEmptyOrEnemy(Position + new Point(1, -1), board, result);
            AddIfEmptyOrEnemy(Position + new Point(-1, 1), board, result);
            AddIfEmptyOrEnemy(Position + new Point(-1, -1), board, result);
        }
        else if (PieceType == PieceType.Pawn)
        {
            var oneStep = Position + Forward(Color);
            if (board.IsEmptySquare(oneStep))
            {
                result.Add(oneStep);

                if (!HasMoved)
                {
                    var twoStep = oneStep + Forward(Color);
                    if (board.IsEmptySquare(twoStep))
                    {
                        result.Add(twoStep);
                    }
                }
            }

            var aheadRight = Position + Forward(Color) + new Point(1, 0);
            var aheadLeft = Position + Forward(Color) + new Point(-1, 0);
            AddIfEnemy(board, aheadLeft, result);
            AddIfEnemy(board, aheadRight, result);
        }

        return result;
    }

    private void AddIfEnemy(ChessBoard board, Point position, List<Point> result)
    {
        var piece = board.GetPieceAt(position);
        if (piece.HasValue && piece.Value.Color != Color)
        {
            result.Add(position);
        }
    }

    private Point Forward(PieceColor color)
    {
        if (color == PieceColor.White)
        {
            return new Point(0, -1);
        }

        return new Point(0, 1);
    }

    private void Project(ChessBoard board, Point step, List<Point> result)
    {
        var currentSquare = Position + step;
        while (board.IsEmptySquare(currentSquare))
        {
            result.Add(currentSquare);
            currentSquare += step;
        }

        var piece = board.GetPieceAt(currentSquare);
        if (piece.HasValue && piece.Value.Color != Color)
        {
            // enemy there, add it!
            result.Add(currentSquare);
        }
    }

    private void AddIfEmptyOrEnemy(Point target, ChessBoard board, List<Point> result)
    {
        var piece = board.GetPieceAt(target);
        if (board.IsEmptySquare(target) || (piece.HasValue && piece.Value.Color != Color))
        {
            result.Add(target);
        }
    }

    public override string ToString()
    {
        return $"[{Id}] {Color} {PieceType} at ({Position.X},{Position.Y})";
    }
}
