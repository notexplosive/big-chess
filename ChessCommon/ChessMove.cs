using Microsoft.Xna.Framework;

namespace ChessCommon;

public class ChessMove
{
    public ChessMove(ChessPiece pieceBeforeMove, Point finalPosition)
    {
        PieceBeforeMove = pieceBeforeMove;
        FinalPosition = finalPosition;
    }

    public ChessPiece PieceBeforeMove { get; }
    public ChessPiece PieceAfterMove => PieceBeforeMove with {Position = FinalPosition};
    public Point FinalPosition { get; }
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
