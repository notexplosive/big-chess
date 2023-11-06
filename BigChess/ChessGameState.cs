using System;
using Microsoft.Xna.Framework;

namespace BigChess;

public class ChessGameState
{
    private int? _promotingPieceId;
    public PieceColor CurrentTurn { get; private set; }
    public int PendingPromotionId => _promotingPieceId ?? -1;
    public bool PlayerCanMovePieces => _playerIsActionable && ! _promotingPieceId.HasValue;
    private bool _playerIsActionable = true;

    public event Action<ChessPiece>? PromotionRequested;

    public void NextTurn()
    {
        CurrentTurn = Constants.FlipColor(CurrentTurn);
    }

    public void StopInput()
    {
        _playerIsActionable = false;
    }

    public void RestoreInput()
    {
        _playerIsActionable = true;
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
