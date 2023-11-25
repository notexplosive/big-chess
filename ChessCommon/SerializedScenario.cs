using System;
using ChessCommon;
using Newtonsoft.Json;

namespace BigChess;

[Serializable]
public class SerializedScenario
{
    [JsonProperty("board")]
    public SerializedBoard Board { get; set; } = new();
    
    [JsonProperty("data")]
    public SerializedBoardData BoardData { get; set; } = new();
}