namespace BigChess;

public class ChessGameState
{
    public PieceColor CurrentTurn { get; private set; }

    public bool PlayerCanInput { get; set; } = true;

    public void NextTurn()
    {
        CurrentTurn = Constants.FlipColor(CurrentTurn);
    }

    public void StopInput()
    {
        PlayerCanInput = false;
    }

    public void RestoreInput()
    {
        PlayerCanInput = true;
    }
}
