using System;
using System.Linq;
using ExplogineMonoGame;

namespace BigChess;

public class ChessGameState
{
    private readonly ChessBoard _board;
    private readonly BoardData _boardData;
    private PieceColor _currentTurn;
    private int? _promotingPieceId;
    public int PendingActionPoints { get; private set; }

    public ChessGameState(ChessBoard board, BoardData boardData)
    {
        _boardData = boardData;
        _board = board;

        boardData.Changed += OnBoardDataChanged;
        OnBoardDataChanged(boardData);
    }

    private void OnBoardDataChanged(BoardData boardData)
    {
        PendingActionPoints = boardData.NumberOfActionPoints;
        CurrentTurn = PieceColor.White;
    }

    public PieceColor CurrentTurn
    {
        get => _currentTurn;
        private set
        {
            _currentTurn = value;
            PendingActionPoints = _boardData.NumberOfActionPoints;
            TurnChanged?.Invoke(value);
        }
    }

    public int PendingPromotionId => _promotingPieceId ?? -1;

    public event Action<ChessPiece>? PromotionRequested;
    public event Action<PieceColor>? TurnChanged;
    public event Action<PieceColor,bool>? GameEnded;
    public event Action? ActionCompleted;

    private void EndTurn()
    {
        var newTurnPlayer = Constants.OppositeColor(CurrentTurn);

        if (_board.HasValidMove(newTurnPlayer))
        {
            ForceNextTurn();
        }
        else
        {
            if (_board.IsInCheck(newTurnPlayer))
            {
                Victory();
            }
            else
            {
                Stalemate();
            }
        }
    }

    public void ForceNextTurn()
    {
        CurrentTurn = Constants.OppositeColor(CurrentTurn);
    }

    private void Stalemate()
    {
        GameEnded?.Invoke(CurrentTurn, false);
    }

    private void Victory()
    {
        GameEnded?.Invoke(CurrentTurn, true);
    }

    public void RequestMovePiece(ChessMove move)
    {
        if (PendingPromotionId != -1)
        {
            Client.Debug.LogWarning("Attempted to move during pending promotion");
            return;
        }

        if (PendingActionPoints <= 0)
        {
            Client.Debug.LogWarning("Attempted to move without action points");
            return;
        }

        _board.Pieces.ExecuteMove(move);
        
        var piece = move.PieceAfterMove;
        if (piece.PieceType == PieceType.Pawn && !_boardData.IsWithinBoard(piece.Position + Constants.Forward(piece.Color)))
        {
            _promotingPieceId = piece.Id;
            PromotionRequested?.Invoke(piece);
        }
        else
        {
            CompleteAction();
        }
    }

    public void FinishPromotePiece(int id, PieceType pieceType)
    {
        _board.Promote(id, pieceType);
        _promotingPieceId = null;
        
        CompleteAction();
    }

    private void CompleteAction()
    {
        ActionCompleted?.Invoke();
        PendingActionPoints--;

        if (PendingActionPoints <= 0)
        {
            EndTurn();
        }
    }
}
