using System;

namespace BigChess;

public class UiState
{
    private readonly ChessGameState _gameState;
    private ChessPiece? _selectedPiece;

    public ChessPiece? SelectedPiece
    {
        get => _selectedPiece;
        set
        {
            _selectedPiece = value;
            SelectionChanged?.Invoke(value);
        }
    }

    public UiState(ChessGameState gameState)
    {
        _gameState = gameState;
    }
    
    public bool PlayerCanMovePieces => _blockInputSemaphore == 0 && _gameState.PendingPromotionId == -1;
    private int _blockInputSemaphore;
    public void StopInput()
    {
        _blockInputSemaphore++;
    }

    public void RestoreInput()
    {
        _blockInputSemaphore--;
        _blockInputSemaphore = Math.Max(_blockInputSemaphore, 0);
    }

    public event Action<ChessPiece?>? SelectionChanged;
}
