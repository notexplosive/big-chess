using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace BigChess;

public readonly record struct BoardRectangle(RectangleF PixelRect, Point GridPosition)
{
    public bool IsLight => GridPosition.X % 2 == GridPosition.Y % 2;
    public int SectionX => GridPosition.X / (Constants.BoardLength / Constants.SectionCount);
    public int SectionY => GridPosition.Y / (Constants.BoardLength / Constants.SectionCount);
    public int SectionId => SectionX * Constants.SectionCount + SectionY + 1;
}
