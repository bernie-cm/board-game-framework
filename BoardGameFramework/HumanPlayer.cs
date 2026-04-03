namespace BoardGameFramework;

public class HumanPlayer : Player
{
    public HumanPlayer(int playerNumber, string gamePiece) : base(playerNumber, gamePiece) {}
    public override (int row, int col, string value) MakeMove(Board board, IDisplay display)
    {
        // Method that gets a move from a Human Player.
        /*
        Show whose turn it is via display.ShowMessage()
        Prompt for row via display.GetInput()
        Prompt for col via display.GetInput()
        Prompt for value via display.GetInput() (the value is what to place — a number for NTT, or the player's Piece for Gomoku/Notakto)
        Validate all inputs are valid integers then check board.IsValidMove(row, col) before returning
        If invalid, show error via display.ShowMessage() and keep looping
        */
        while (true)
        {
            string rowInput = display.GetInput("Enter row: "); // This returns a string
            if (!int.TryParse(rowInput, out int row))
            {
                display.ShowMessage("Input is not valid. Please enter a number.");
                continue;
            }
            row -= 1; //I prefer the row to start at 1 but didn't want to change entire codebase
            string colInput = display.GetInput("Enter col: "); // This returns a string
            if (!int.TryParse(colInput, out int col))
            {
                display.ShowMessage("Input is not valid. Please enter a number.");
                continue;
            }
            col -= 1; //I prefer the col to start at 1 but didn't want to change entire codebase
            string valueInput = display.GetInput("Enter value: "); // This returns a string for the value and doesn't need to be parsed for an int
            if (!board.IsValidMove(row, col))
            {
                display.ShowMessage("This move is not valid. Try again with a different position.");
                continue;
            } else
            {
                return (row, col, valueInput);
            }
        }
    }
}