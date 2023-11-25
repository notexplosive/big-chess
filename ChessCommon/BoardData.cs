using BigChess;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace ChessCommon;

[Serializable]
public class BoardData
{
    private SerializedBoardData _serialized = new();
    public int SectionCount => _serialized.SectionCount;
    public int BoardLength
    {
        get => _serialized.BoardLength;
        set
        {
            _serialized.BoardLength = Math.Max(value, 1);
            Changed?.Invoke(this);
        }
    }

    public int NumberOfActionPoints
    {
        get => _serialized.NumberOfActionPoints;
        set
        {
            _serialized.NumberOfActionPoints = Math.Max(value, 1);
            Changed?.Invoke(this);
        }
    }

    public event Action<BoardData>? Changed;

    public Point TotalBoardSizePixels()
    {
        return new Point(BoardLength * Constants.TileSizePixels);
    }

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

    public void PopulateFromScenario(SerializedScenario scenario)
    {
        _serialized = scenario.BoardData;
        Changed?.Invoke(this);
    }

    public SerializedBoardData Serialize()
    {
        return _serialized;
    }
}
