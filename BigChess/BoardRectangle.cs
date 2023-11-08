using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace BigChess;

public readonly record struct BoardRectangle(BoardData BoardData, RectangleF PixelRect, Point GridPosition)
{
    public bool IsLight => GridPosition.X % 2 == GridPosition.Y % 2;
    public int SectionX => GridPosition.X / (BoardData.BoardLength / BoardData.SectionCount);
    public int SectionY => GridPosition.Y / (BoardData.BoardLength / BoardData.SectionCount);
    public int SectionId => SectionX * BoardData.SectionCount + SectionY + 1;
}
