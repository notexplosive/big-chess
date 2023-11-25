using BigChess;
using Newtonsoft.Json;

namespace ChessCommon;

[Serializable]
public class SerializedChessPiece
{
    [JsonProperty("position")]
    public SerializedGridPosition Position { get; set; } = new();

    [JsonProperty("type")]
    public PieceType Type { get; set; }
    
    [JsonProperty("color")]
    public PieceColor Color { get; set; }

    public ChessPiece Deserialize()
    {
        return new ChessPiece
        {
            PieceType = Type,
            Color = Color,
            Position = Position.ToPoint()
        };
    }
}
