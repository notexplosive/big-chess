using System;
using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using Microsoft.Xna.Framework;

namespace BigChess;

public class ChessInput
{
    private Point? _hoveredSquare;
    private bool _isDragging;
    private MouseButton? _primedButton;
    private readonly UiState _uiState;

    public ChessInput(UiState uiState)
    {
        _uiState = uiState;
    }

    public Point? PrimedSquare { get; private set; }

    public event Action<Point, Point>? DragSucceeded;
    public event Action? DragCancelled;
    public event Action<Point?>? DragFinished;
    public event Action<Point, MouseButton>? SquareClicked;
    public event Action<Point>? SquareHovered;
    public event Action<Point>? DragInitiated;

    public void SetHoveredSquare(ConsumableInput input, Point gridPosition)
    {
        if (_hoveredSquare != gridPosition)
        {
            SquareHovered?.Invoke(gridPosition);
            _hoveredSquare = gridPosition;
        }

        if (!_uiState.PlayerCanMovePieces)
        {
            return;
        }
        
        if (input.Mouse.GetButton(MouseButton.Left).WasPressed)
        {
            input.Mouse.Consume(MouseButton.Left);
            
            PrimedSquare = gridPosition;
            _primedButton = MouseButton.Left;
            _isDragging = true;

            DragInitiated?.Invoke(gridPosition);
        }
        
        if (input.Mouse.GetButton(MouseButton.Left).WasReleased)
        {
            input.Mouse.Consume(MouseButton.Left);

            if (_primedButton == MouseButton.Left)
            {
                if (PrimedSquare == gridPosition)
                {
                    SquareClicked?.Invoke(gridPosition, MouseButton.Left);
                }
                else if (PrimedSquare.HasValue)
                {
                    DragSucceeded?.Invoke(PrimedSquare.Value, gridPosition);
                }

                if (_isDragging)
                {
                    _isDragging = false;
                    DragFinished?.Invoke(gridPosition);
                }
            }

            PrimedSquare = null;
            _primedButton = null;
        }

        if (input.Mouse.GetButton(MouseButton.Right).WasPressed)
        {
            input.Mouse.Consume(MouseButton.Right);
            PrimedSquare = gridPosition;
            _primedButton = MouseButton.Right;
        }

        if (input.Mouse.GetButton(MouseButton.Right).WasReleased)
        {
            input.Mouse.Consume(MouseButton.Right);

            if (_primedButton == MouseButton.Right)
            {
                if (PrimedSquare == gridPosition)
                {
                    SquareClicked?.Invoke(gridPosition, MouseButton.Right);
                }
            }

            PrimedSquare = null;
            _primedButton = null;
        }
    }

    public void OnHoverVoid(ConsumableInput input)
    {
        if (input.Mouse.GetButton(MouseButton.Left).WasReleased && _isDragging)
        {
            _isDragging = false;
            DragCancelled?.Invoke();
            DragFinished?.Invoke(null);
        }
    }
}
