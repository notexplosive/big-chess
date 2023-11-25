using Newtonsoft.Json;

namespace ChessCommon;

[Serializable]
public class SerializedBoardData
{
    [JsonProperty("section_count")]
    public int SectionCount { get; set; } = 1;
    
    [JsonProperty("board_length")]
    public int BoardLength { get; set; } = 8;
    
    [JsonProperty("action_points")]
    public int NumberOfActionPoints { get; set; } = 1;
}
