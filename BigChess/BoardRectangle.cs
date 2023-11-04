using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace BigChess;

public readonly record struct BoardRectangle(RectangleF PixelRect, Point GridPosition)
{
    public bool IsLight => GridPosition.X % 2 != GridPosition.Y % 2;
}
