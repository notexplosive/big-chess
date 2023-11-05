using System.Collections.Generic;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace BigChess;

public readonly record struct ChessPiece
{
    public int Id { get; init; }
    public Point Position { get; init; }
    public PieceType PieceType { get; init; }
    public bool HasMoved { get; init; }
    public PieceColor Color { get; init; }

    public List<ChessMove> GetValidMoves(ChessBoard board)
    {
        var result = new List<ChessMove>();

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
            ScanAndAdd(board, new Point(1, 1), result);
            ScanAndAdd(board, new Point(1, -1), result);
            ScanAndAdd(board, new Point(-1, 1), result);
            ScanAndAdd(board, new Point(-1, -1), result);
        }
        else if (PieceType == PieceType.Rook)
        {
            ScanAndAdd(board, new Point(0, 1), result);
            ScanAndAdd(board, new Point(0, -1), result);
            ScanAndAdd(board, new Point(-1, 0), result);
            ScanAndAdd(board, new Point(1, 0), result);
        }
        else if (PieceType == PieceType.Queen)
        {
            ScanAndAdd(board, new Point(0, 1), result);
            ScanAndAdd(board, new Point(0, -1), result);
            ScanAndAdd(board, new Point(-1, 0), result);
            ScanAndAdd(board, new Point(1, 0), result);
            ScanAndAdd(board, new Point(1, 1), result);
            ScanAndAdd(board, new Point(1, -1), result);
            ScanAndAdd(board, new Point(-1, 1), result);
            ScanAndAdd(board, new Point(-1, -1), result);
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

            if (!HasMoved)
            {
                var scannedCardinals = new List<ChessPiece?>
                {
                    Scan(board, new Point(0, 1)),
                    Scan(board, new Point(0, -1)),
                    Scan(board, new Point(-1, 0)),
                    Scan(board, new Point(1, 0))
                };

                foreach (var scannedPiece in scannedCardinals)
                {
                    if (scannedPiece.HasValue && scannedPiece.Value.Color == Color &&
                        scannedPiece.Value.PieceType == PieceType.Rook && !scannedPiece.Value.HasMoved)
                    {
                        // hacky hack, there are lots of other ways we could calculate these positions, I'm just lazy
                        var rookNewSpot = Position + (scannedPiece.Value.Position - Position).ToVector2().Normalized()
                            .ToPoint();
                        var myNewSpot = Position + ((scannedPiece.Value.Position - Position).ToVector2().Normalized() * 2)
                            .ToPoint();

                        var piece = board.GetPieceAt(myNewSpot);
                        if (board.IsEmptySquare(myNewSpot) || (piece.HasValue && piece.Value.Color != Color))
                        {
                            result.Add(ChessMove.Castle(this, myNewSpot, new ChessMove(scannedPiece.Value, rookNewSpot)));
                        }
                    }
                }
            }
        }
        else if (PieceType == PieceType.Pawn)
        {
            var oneStep = Position + Constants.Forward(Color);
            if (board.IsEmptySquare(oneStep))
            {
                result.Add(ChessMove.Normal(this, oneStep));

                if (!HasMoved)
                {
                    var extraPawnStep = oneStep + Constants.Forward(Color);
                    if (board.IsEmptySquare(extraPawnStep))
                    {
                        result.Add(ChessMove.Normal(this, extraPawnStep));
                    }
                }
            }

            var aheadRight = Position + Constants.Forward(Color) + new Point(1, 0);
            var aheadLeft = Position + Constants.Forward(Color) + new Point(-1, 0);
            AddIfEnemy(board, aheadLeft, result);
            AddIfEnemy(board, aheadRight, result);
        }

        return result;
    }

    private void AddIfEnemy(ChessBoard board, Point position, List<ChessMove> result)
    {
        var piece = board.GetPieceAt(position);
        if (piece.HasValue && piece.Value.Color != Color)
        {
            result.Add(ChessMove.Normal(this, position));
        }
    }

    private ChessPiece? Scan(ChessBoard board, Point step)
    {
        var currentSquare = Position + step;
        while (board.IsEmptySquare(currentSquare))
        {
            currentSquare += step;
        }

        var piece = board.GetPieceAt(currentSquare);
        if (piece.HasValue)
        {
            return piece;
        }

        return null;
    }

    private void ScanAndAdd(ChessBoard board, Point step, List<ChessMove> result)
    {
        var currentSquare = Position + step;
        while (board.IsEmptySquare(currentSquare))
        {
            result.Add(ChessMove.Normal(this, currentSquare));
            currentSquare += step;
        }

        var piece = board.GetPieceAt(currentSquare);
        if (piece.HasValue && piece.Value.Color != Color)
        {
            // enemy there, add it!
            result.Add(ChessMove.Normal(this, currentSquare));
        }
    }

    private void AddIfEmptyOrEnemy(Point target, ChessBoard board, List<ChessMove> result)
    {
        var piece = board.GetPieceAt(target);
        if (board.IsEmptySquare(target) || (piece.HasValue && piece.Value.Color != Color))
        {
            result.Add(ChessMove.Normal(this, target));
        }
    }

    public override string ToString()
    {
        return $"[{Id}] {Color} {PieceType} at ({Position.X},{Position.Y})";
    }
}
