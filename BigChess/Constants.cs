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

    public static Point RenderResolution => new Point(Constants.RenderWidth, Constants.RenderHeight);
    public static Point TotalBoardSizePixels => new Point(Constants.BoardLength * TileSize);
}