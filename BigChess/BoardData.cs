using System.Collections.Generic;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace BigChess;

public class BoardData
{

    public int SectionCount => 1;
    public int BoardLength => 6; //8 * SectionCount;
    public Point TotalBoardSizePixels => new(BoardLength * Constants.TileSizePixels);
    public int NumberOfActionPoints => 1;

    public IEnumerable<BoardRectangle> BoardRectangles()
    {
        for (var y = 0; y < BoardLength; y++)
        {
            for (var x = 0; x < BoardLength; x++)
            {
                yield return new BoardRectangle(
                    this,
                    new RectangleF(new Vector2(x, y) * Constants.TileSizePixels,
                        new Vector2(Constants.TileSizePixels)),
                    new Point(x, y));
            }
        }
    }


    public bool IsWithinBoard(Point position)
    {
        return position.X >= 0 && position.Y >= 0 && position.X < BoardLength &&
               position.Y < BoardLength;
    }

}
