using System;
using System.Collections.Generic;
using System.Linq;
using ChessCommon;
using ExplogineMonoGame;
using ExTween;
using Microsoft.Xna.Framework;

namespace BigChess;

public class EditorCommandsPrompt : ButtonListPrompt
{
    private readonly BoardData _boardData;
    private readonly ChessBoard _chessBoard;
    private bool _isOpen;

    public EditorCommandsPrompt(IRuntime runtime, ChessBoard chessBoard, BoardData boardData) : base(runtime, "Editor")
    {
        _chessBoard = chessBoard;
        _boardData = boardData;
    }

    public override bool IsOpen => _isOpen;

    public void Open()
    {
        _isOpen = true;
        Refresh();
    }

    private void Refresh()
    {
        var buttons = new List<IEditorOption>();

        buttons.Add(new ButtonTemplate("Mirror White Vertically (Delete Black)", () => Mirror(PieceColor.White)));
        buttons.Add(new ButtonTemplate("Mirror Black Vertically (Delete White)", () => Mirror(PieceColor.Black)));
        buttons.Add(new SliderTemplate(x=>$"Board Width: {x}",
            new TweenableInt(() => _boardData.BoardLength, x => _boardData.BoardLength = x)));
        buttons.Add(new SliderTemplate(x=>$"Moves Per Turn: {x}",
            new TweenableInt(() => _boardData.NumberOfActionPoints, x =>
            {
                _boardData.NumberOfActionPoints = x;
            })));

        GenerateButtons(buttons);
    }

    private void Mirror(PieceColor color)
    {
        var oppositeColor = Constants.OppositeColor(color);

        var ids = _chessBoard.Pieces.GetAllIds().ToList();

        foreach (var id in ids)
        {
            if (_chessBoard.Pieces.GetPieceFromId(id)!.Value.Color == oppositeColor)
            {
                _chessBoard.Pieces.DeletePiece(id);
            }
        }

        // refresh list, post cull
        ids = _chessBoard.Pieces.GetAllIds().ToList();

        foreach (var id in ids)
        {
            var piece = _chessBoard.Pieces.GetPieceFromId(id)!.Value;
            var newY = _boardData.BoardLength - piece.Position.Y - 1;

            if (piece.Position.Y > _boardData.BoardLength / 2)
            {
                newY = _boardData.BoardLength - piece.Position.Y - 1;
            }

            var newPosition = new Point(piece.Position.X, newY);

            if (_chessBoard.Pieces.GetPieceAt(newPosition) == null)
            {
                _chessBoard.Pieces.AddPiece(new ChessPiece
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

    public override void Cancel()
    {
        Close();
    }
}
