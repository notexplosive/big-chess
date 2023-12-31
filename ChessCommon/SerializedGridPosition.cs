using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace ChessCommon;

[Serializable]
public class SerializedGridPosition
{
    public static SerializedGridPosition FromPoint(Point point)
    {
        return new SerializedGridPosition {X = point.X, Y = point.Y};
    }
    
    [JsonProperty("x")]
    public int X { get; set; }
    
    [JsonProperty("y")]
    public int Y { get; set; }

    public Point ToPoint()
    {
        return new Point(X, Y);
    }
}
