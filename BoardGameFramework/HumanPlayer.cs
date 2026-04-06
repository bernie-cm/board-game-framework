namespace BoardGameFramework.Core;

// Concrete Player implementation for human-controlled players.
// All input and validation is handled here so game classes only need to call MakeMove
// and can trust they will receive a valid board position in return.
public class HumanPlayer : Player
{
    public HumanPlayer(int playerNumber, string gamePiece) : base(playerNumber, gamePiece) {}

    // Prompts the player for a row, column, and value, repeating until a valid position is given.
    // Row and column are entered as 1-based to match what the player sees on screen,
    // then converted to 0-based internally before returning.
    public override (int row, int col, string value) MakeMove(Board board, IDisplay display)
    {
        while (true)
        {
            string rowInput = display.GetInput("Enter row: ");
            if (!int.TryParse(rowInput, out int row))
            {
                display.ShowMessage("Input is not valid. Please enter a number.");
                continue;
            }
            row -= 1;

            string colInput = display.GetInput("Enter col: ");
            if (!int.TryParse(colInput, out int col))
            {
                display.ShowMessage("Input is not valid. Please enter a number.");
                continue;
            }
            col -= 1;

            // Value is read as a raw string so this method stays generic across game types
            string valueInput = display.GetInput("Enter value: ");

            if (!board.IsValidMove(row, col))
            {
                display.ShowMessage("This move is not valid. Try again with a different position.");
                continue;
            }

            return (row, col, valueInput);
        }
    }
}
