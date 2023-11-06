using System;
using Microsoft.Xna.Framework;

namespace BigChess;

public class ChessGameState
{
    private int? _promotingPieceId;
    public PieceColor CurrentTurn { get; private set; }
    public int PendingPromotionId => _promotingPieceId ?? -1;

    public event Action<ChessPiece>? PromotionRequested;

    public void NextTurn()
    {
        CurrentTurn = Constants.FlipColor(CurrentTurn);
    }

    public void OnPieceMoved(ChessMove move)
    {
        var piece = move.PieceAfterMove;
        if (piece.PieceType == PieceType.Pawn)
        {
            if (!Constants.IsWithinBoard(piece.Position + Constants.Forward(piece.Color)))
            {
                _promotingPieceId = piece.Id;
                PromotionRequested?.Invoke(piece);
            }
        }
    }

    public void OnPiecePromoted(ChessPiece piece)
    {
        _promotingPieceId = null;
    }
}
