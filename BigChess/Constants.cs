using System.Collections.Generic;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace BigChess;

public static class Constants
{
    public static int PieceRenderSize => 426;
    public static int TileSize => 64;
    public static int BoardLength => 40;

    public static int RenderWidth => 1920;
    public static int RenderHeight => 1080;

    public static Point RenderResolution => new(Constants.RenderWidth, Constants.RenderHeight);
    public static Point TotalBoardSizePixels => new(Constants.BoardLength * Constants.TileSize);

    public static IEnumerable<BoardRectangle> BoardRectangles()
    {
        for (var y = 0; y < Constants.BoardLength; y++)
        {
            for (var x = 0; x < Constants.BoardLength; x++)
            {
                yield return new BoardRectangle(
                    new RectangleF(new Vector2(x, y) * Constants.TileSize, new Vector2(Constants.TileSize)),
                    new Point(x, y));
            }
        }
    }

    public static RectangleF PixelRectangle(Point position)
    {
        return new RectangleF(position.ToVector2() * Constants.TileSize, new Vector2(Constants.TileSize));
    }

    public static int FrameIndex(PieceType type, PieceColor color)
    {
        switch (type, color)
        {
            case (PieceType.King, PieceColor.White):
                return 0;
            case (PieceType.Queen, PieceColor.White):
                return 1;
            case (PieceType.Bishop, PieceColor.White):
                return 2;
            case (PieceType.Knight, PieceColor.White):
                return 3;
            case (PieceType.Rook, PieceColor.White):
                return 4;
            case (PieceType.Pawn, PieceColor.White):
                return 5;

            case (PieceType.King, PieceColor.Black):
                return 6;
            case (PieceType.Queen, PieceColor.Black):
                return 7;
            case (PieceType.Bishop, PieceColor.Black):
                return 8;
            case (PieceType.Knight, PieceColor.Black):
                return 9;
            case (PieceType.Rook, PieceColor.Black):
                return 10;
            case (PieceType.Pawn, PieceColor.Black):
                return 11;
        }

        return 0;
    }

    public static bool IsWithinBoard(Point position)
    {
        return position.X >= 0 && position.Y >= 0 && position.X < Constants.BoardLength &&
               position.Y < Constants.BoardLength;
    }

    public static Vector2 ToWorldPosition(Point gridPosition)
    {
        return gridPosition.ToVector2() * Constants.TileSize;
    }

    public static PieceColor FlipColor(PieceColor color)
    {
        return color switch
        {
            PieceColor.White => PieceColor.Black,
            PieceColor.Black => PieceColor.White,
            _ => color
        };
    }
}
