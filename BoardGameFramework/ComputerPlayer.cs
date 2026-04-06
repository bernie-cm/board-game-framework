namespace BoardGameFramework.Core;

public class ComputerPlayer : Player
{
    // This private field stores the computer move logic interface
    private IComputerStrategy _strategy;
    public ComputerPlayer(int playerNumber, string gamePiece, IComputerStrategy strategy) : base(playerNumber, gamePiece)
    {
        _strategy = strategy;
    }
    public override (int row, int col, string value) MakeMove(Board board, IDisplay display)
    {
        /*
        Calls strategy.ChooseMove(board, Piece). 
        Displays what the computer chose via display.ShowMessage(). 
        Returns the result
        */
        var computerMove = _strategy.ChooseMove(board, GamePiece);
        display.ShowMessage($"Computer Player has played {computerMove.value} at ({computerMove.row}, {computerMove.col}).");
        return computerMove;
    }
}