using BigChess;
using Newtonsoft.Json;

namespace ChessCommon;

[Serializable]
public class SerializedBoard
{
    [JsonProperty("pieces")]
    public List<SerializedChessPiece> Pieces { get; set; } = new();
}
