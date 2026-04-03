namespace BoardGameFramework;

public class ComputerPlayer : Player
{
    // This private field stores the computer move logic interface
    private IComputerStrategy strategy;
    public ComputerPlayer(int playerNumber, string gamePiece, IComputerStrategy strategy) : base(playerNumber, gamePiece)
    {
        this.strategy = strategy;
    }
    public override (int row, int col, string value) MakeMove(Board board, IDisplay display)
    {
        /*
        Calls strategy.ChooseMove(board, Piece). 
        Displays what the computer chose via display.ShowMessage(). 
        Returns the result
        */
        var computerMove = strategy.ChooseMove(board, GamePiece);
        display.ShowMessage($"Computer Player has played {computerMove.value} at ({computerMove.row}, {computerMove.col}).");
        return computerMove;
    }
}