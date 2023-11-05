using Microsoft.Xna.Framework;

namespace BigChess;

public class ChessMove
{
    public ChessMove(ChessPiece pieceBeforeMove, Point position)
    {
        PieceBeforeMove = pieceBeforeMove;
        Position = position;
    }

    public ChessPiece PieceBeforeMove { get; }
    public ChessPiece PieceAfterMove => PieceBeforeMove with {Position = Position};
    public Point Position { get; }
    public ChessMove? NextMove { get; init; }

    public static ChessMove Normal(ChessPiece pieceBeforeMove, Point position)
    {
        return new ChessMove(pieceBeforeMove, position);
    }

    public static ChessMove Castle(ChessPiece pieceBeforeMove, Point position, ChessMove nextMove)
    {
        return new ChessMove(pieceBeforeMove, position) {NextMove = nextMove};
    }
}
