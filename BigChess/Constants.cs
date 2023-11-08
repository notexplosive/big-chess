using System;
using System.Collections.Generic;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Gui;
using Microsoft.Xna.Framework;

namespace BigChess;

public static class Constants
{
    public static int PieceSizePixels => 426;
    public static int TileSizePixels => 64;
    public static int StandardChessBoardLength => 8;
    public static int SectionCount => 5;
    public static int BoardLength => StandardChessBoardLength * SectionCount;

    public static int RenderWidth => 1920;
    public static int RenderHeight => 1080;
    
    public static Point RenderResolution => new(Constants.RenderWidth, Constants.RenderHeight);
    public static Point TotalBoardSizePixels => new(Constants.BoardLength * Constants.TileSizePixels);
    public static int NumberOfActionPoints => 5;

    public static IEnumerable<BoardRectangle> BoardRectangles()
    {
        for (var y = 0; y < Constants.BoardLength; y++)
        {
            for (var x = 0; x < Constants.BoardLength; x++)
            {
                yield return new BoardRectangle(
                    new RectangleF(new Vector2(x, y) * Constants.TileSizePixels, new Vector2(Constants.TileSizePixels)),
                    new Point(x, y));
            }
        }
    }

    public static RectangleF PixelRectangle(Point position)
    {
        return new RectangleF(position.ToVector2() * Constants.TileSizePixels, new Vector2(Constants.TileSizePixels));
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
        return gridPosition.ToVector2() * Constants.TileSizePixels;
    }

    public static PieceColor OppositeColor(PieceColor color)
    {
        return color switch
        {
            PieceColor.White => PieceColor.Black,
            PieceColor.Black => PieceColor.White,
            _ => color
        };
    }

    public static Point Forward(PieceColor color)
    {
        if (color == PieceColor.White)
        {
            return new Point(0, -1);
        }

        return new Point(0, 1);
    }

    public static IGuiTheme Theme { get; } = new SimpleGuiTheme(Color.White, Color.Black, Color.Transparent, new IndirectFont("engine/logo-font", 32), selectionColor: Color.Cyan);

    public static Color PieceColorToRgb(PieceColor color)
    {
        return color switch
        {
            PieceColor.White => Color.White,
            PieceColor.Black => Color.Black,
            _ => Color.Red
        };
    }
}
