using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using Microsoft.Xna.Framework;

namespace BigChess;

public class EditorCommandsPrompt : ButtonListPrompt
{
    private readonly ChessBoard _chessBoard;
    private bool _isOpen;

    public EditorCommandsPrompt(IRuntime runtime, ChessBoard chessBoard) : base(runtime, "Editor")
    {
        _chessBoard = chessBoard;
    }

    public override bool IsOpen => _isOpen;

    public void Open()
    {
        _isOpen = true;
        Refresh();
    }

    private void Refresh()
    {
        var buttons = new List<ButtonTemplate>();

        buttons.Add(new ButtonTemplate("Mirror White Vertically (Delete Black)", () => Mirror(PieceColor.White)));
        buttons.Add(new ButtonTemplate("Mirror Black Vertically (Delete White)", () => Mirror(PieceColor.Black)));

        GenerateButtons(buttons);
    }

    private void Mirror(PieceColor color)
    {
        var oppositeColor = Constants.OppositeColor(color);

        var ids = _chessBoard.GetAllIds().ToList();

        foreach (var id in ids)
        {
            if (_chessBoard.GetPieceFromId(id)!.Value.Color == oppositeColor)
            {
                _chessBoard.DeletePiece(id);
            }
        }
        
        // refresh list, post cull
        ids = _chessBoard.GetAllIds().ToList();

        foreach (var id in ids)
        {
            var piece = _chessBoard.GetPieceFromId(id)!.Value;
            var newY = Constants.BoardLength - piece.Position.Y - 1; 

            if (piece.Position.Y > Constants.BoardLength / 2)
            {
                newY = Constants.BoardLength - piece.Position.Y - 1;
            }

            var newPosition = new Point(piece.Position.X,newY);

            if (_chessBoard.GetPieceAt(newPosition) == null)
            {
                _chessBoard.AddPiece(new ChessPiece
                    {Color = oppositeColor, PieceType = piece.PieceType, Position = newPosition});
            }
            else
            {
                Client.Debug.LogWarning($"Attempted to overwrite piece at {newPosition}");
            }
        }

        Close();
    }

    private void Close()
    {
        _isOpen = false;
    }

    protected override void Cancel()
    {
        Close();
    }
}
