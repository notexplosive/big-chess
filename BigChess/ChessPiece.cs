using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace BigChess;

public class ChessPiece
{
    public Point Position { get; set; }
    public RectangleF PixelRectangle => Constants.PixelRectangle(Position);
    
    public PieceType PieceType { get; set; }
    public PieceColor Color { get; set; }
}