using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BigChess;

[Serializable]
public class SerializedBoard
{
    [JsonProperty("pieces")]
    public List<SerializedChessPiece> Pieces { get; set; } = new();
}
