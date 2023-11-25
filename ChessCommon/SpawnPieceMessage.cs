using NetChess;

namespace ChessCommon;

[Serializable]
public class SpawnPieceMessage : ClientMessage<SpawnPieceMessage>
{
    public int PieceId { get; set; }

    public SerializedChessPiece Piece { get; set; } = new();
}
