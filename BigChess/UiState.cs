using System;

namespace BigChess;

public class UiState
{
    public ChessPiece? SelectedPiece { get => _selectedPiece;
        set
        {
            _selectedPiece = value;
            SelectionChanged?.Invoke(value);
        }
    }

    public event Action<ChessPiece?>? SelectionChanged;

    private ChessPiece? _selectedPiece;
}
