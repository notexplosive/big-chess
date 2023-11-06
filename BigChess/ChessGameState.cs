using System;
using Microsoft.Xna.Framework;

namespace BigChess;

public class ChessGameState
{
    private int? _promotingPieceId;
    private PieceColor _currentTurn;

    public PieceColor CurrentTurn
    {
        get => _currentTurn;
        private set
        {
            _currentTurn = value;
            TurnChanged?.Invoke(value);
        }
    }

    public int PendingPromotionId => _promotingPieceId ?? -1;

    public event Action<ChessPiece>? PromotionRequested;
    public event Action<PieceColor>? TurnChanged;

    public void NextTurn()
    {
        CurrentTurn = Constants.OppositeColor(CurrentTurn);
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
