using System;
using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using Microsoft.Xna.Framework;

namespace BigChess;

public class ChessInput
{
    public Point? PrimedSquare { get; private set; }

    public event Action<Point, Point>? DragComplete;
    public event Action? DragCancelled;
    public event Action<Point>? ClickedSquare;
    public event Action<Point>? DragInitiated;

    public void OnHoverSquare(ConsumableInput input, Point gridPosition)
    {
        if (input.Mouse.GetButton(MouseButton.Left).WasPressed)
        {
            input.Mouse.Consume(MouseButton.Left);
            PrimedSquare = gridPosition;

            DragInitiated?.Invoke(gridPosition);
        }
        
        if (input.Mouse.GetButton(MouseButton.Left).WasReleased)
        {
            input.Mouse.Consume(MouseButton.Left);
            
            if (PrimedSquare == gridPosition)
            {
                ClickedSquare?.Invoke(gridPosition);
            }
            else if(PrimedSquare.HasValue)
            {
                DragComplete?.Invoke(PrimedSquare.Value, gridPosition);
            }
            
            PrimedSquare = null;
        }
    }

    public void OnHoverVoid(ConsumableInput input)
    {
        if (input.Mouse.GetButton(MouseButton.Left).WasReleased)
        {
            DragCancelled?.Invoke();
        }
    }
}
