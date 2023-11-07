using System;
using ExplogineMonoGame;

namespace BigChess;

public class ChessGameState
{
    private readonly ChessBoard _board;
    private PieceColor _currentTurn;
    private int? _promotingPieceId;

    public ChessGameState(ChessBoard board)
    {
        _board = board;
    }

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

    public void RequestMovePiece(ChessMove move)
    {
        if (PendingPromotionId != -1)
        {
            Client.Debug.LogWarning("Attempted to move during pending promotion");
            return;
        }

        _board.ForceMovePiece(move);
        
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

    public void PromotePiece(int id, PieceType pieceType)
    {
        _board.Promote(id, pieceType);
        _promotingPieceId = null;
    }
}
