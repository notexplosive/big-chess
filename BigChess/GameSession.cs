using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using Microsoft.Xna.Framework;

namespace BigChess;

public class GameSession : Session
{
    private readonly ChessBoard _board;
    private readonly DiegeticUi _diegeticUi;
    private readonly ChessGameState _gameState;
    private readonly PromotionPrompt _promotionPrompt;
    private readonly UiState _uiState;

    public GameSession(ChessGameState gameState, UiState uiState, ChessBoard board, DiegeticUi diegeticUi,
        PromotionPrompt promotionPrompt)
    {
        _gameState = gameState;
        _uiState = uiState;
        _board = board;
        _diegeticUi = diegeticUi;
        _promotionPrompt = promotionPrompt;

        _gameState.PromotionRequested += RequestPromotion;
    }

    private void RequestPromotion(ChessPiece piece)
    {
        _promotionPrompt.Request(type => { _gameState.FinishPromotePiece(_gameState.PendingPromotionId, type); });
    }

    public override void DragInitiated(Point position)
    {
        var piece = _board.Pieces.GetPieceAt(position);

        if (IsSelectable(piece))
        {
            AttemptSelect(piece);
            _diegeticUi.BeginDrag(piece!.Value);
        }
    }

    private bool AttemptSelect(ChessPiece? piece)
    {
        if (IsSelectable(piece))
        {
            _uiState.SelectedPiece = piece;
            return true;
        }

        return false;
    }

    private bool IsSelectable(ChessPiece? piece)
    {
        return piece.HasValue && piece.Value.Color == _gameState.CurrentTurn;
    }

    public override void ClickOn(Point position, MouseButton mouseButton)
    {
        _diegeticUi.ClearDrag();
        var piece = _board.Pieces.GetPieceAt(position);

        if (piece == _uiState.SelectedPiece)
        {
            return;
        }

        var move = GetMoveForSelectedPiece(position);
        if (move != null)
        {
            _gameState.RequestMovePiece(move);
            _uiState.SelectedPiece = null;
        }
        else
        {
            if (!AttemptSelect(piece))
            {
                _uiState.SelectedPiece = null;
            }
        }
    }

    private ChessMove? GetMoveForSelectedPiece(Point position)
    {
        if (!_uiState.SelectedPiece.HasValue)
        {
            return null;
        }

        var selectedPiece = _uiState.SelectedPiece.Value;
        var validMoves = selectedPiece.GetPermittedMoves(_board);
        var index = validMoves.FindIndex(move => move.FinalPosition == position);
        if (index == -1)
        {
            return null;
        }

        return validMoves[index];

    }

    public override void DragSucceeded(Point position)
    {
        var move = GetMoveForSelectedPiece(position);
        if (move != null)
        {
            _gameState.RequestMovePiece(move);
            _uiState.SelectedPiece = null;
        }
        else
        {
            DragFinished(position);
        }
    }

    public override void DragFinished(Point? position)
    {
        _diegeticUi.ClearDrag();

        if (_uiState.SelectedPiece.HasValue && _uiState.SelectedPiece.Value.Position != position)
        {
            _uiState.SelectedPiece = null;
        }
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack screenLayer)
    {
    }

    public override void OnExit()
    {
    }

    public override void OnEnter()
    {
    }
}
