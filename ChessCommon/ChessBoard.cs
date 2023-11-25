using Microsoft.Xna.Framework;

namespace ChessCommon;

public class ChessBoard
{
    private readonly BoardData _boardData;
    public ChessPieceCollection Pieces { get; init; } = new();
    public event Action<ChessPiece>? PiecePromoted;

    public ChessBoard(BoardData boardData)
    {
        _boardData = boardData;
    }
    
    public bool IsEmptySquare(Point position)
    {
        return _boardData.IsWithinBoard(position) && Pieces.GetPieceAt(position) == null;
    }
    
    public void Promote(int id, PieceType pieceType)
    {
        if (Pieces.TryFindId(id, out var oldPiece))
        {
            Pieces.DeletePiece(id);
            var piece = Pieces.AddPiece(oldPiece with {PieceType = pieceType});
            PiecePromoted?.Invoke(piece);
        }
        else
        {
            // Client.Debug.LogWarning($"Tried to promote {id} but it does not exist");
        }
    }
    
    public bool IsInCheck(PieceColor color)
    {
        if (Pieces.Count(PieceType.King, color) > 1)
        {
            return false;
        }

        foreach (var piece in Pieces.All())
        {
            if (piece.PieceType == PieceType.King && piece.Color == color)
            {
                if (IsThreatened(piece.Position, piece.Color))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsThreatened(Point piecePosition, PieceColor defendingColor)
    {
        var attackingColor = Constants.OppositeColor(defendingColor);

        foreach (var piece in Pieces.All())
        {
            if (piece.Color == attackingColor)
            {
                foreach (var move in piece.GetNormalMoves(this))
                {
                    if (move.FinalPosition == piecePosition)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public ChessBoard Clone()
    {
        return new ChessBoard(_boardData)
        {
            Pieces = Pieces.Clone()
        };
    }

    public bool HasValidMove(PieceColor color)
    {
        foreach (var piece in Pieces.All())
        {
            if (piece.Color == color)
            {
                if(piece.GetPermittedMoves(this).Count > 0)
                {
                    // If we found ANY valid move
                    return true;
                }
            }
        }
        
        return false;
    }
}
