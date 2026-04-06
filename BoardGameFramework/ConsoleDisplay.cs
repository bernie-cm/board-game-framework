using BoardGameFramework.Core;

namespace BoardGameUI;

public class ConsoleDisplay : IDisplay
{
    /*
    ConsoleDisplay is the only class in the entire framework that uses Console.WriteLine and Console.ReadLine. 
    Every other class communicates with the user through this interface.
    */
    public void ShowBoard(Board board)
    {
        // Method displays the board to the user
        // loop through board.Rows and board.Cols
            // each iteration call board.GetCell(row, col) for every row and column
                // print the grid
        // Display the board with separators. Empty cells show "." Filled cells should show the number
        for (int row = 0; row < board.Rows; row++) {
            for (int col = 0; col < board.Cols; col++) {
                if (board.IsCellEmpty(row, col)) {
                    Console.Write(" . ");
                } else {
                    Console.Write($" {board.GetCell(row, col)} ");
                }
                if (col < board.Cols - 1) {
                    Console.Write(" | ");
                }
            }
            Console.WriteLine(); // Prints a new line after each row
            // Separator line between rows
            if (row < board.Rows - 1) {
                int width = (board.Cols * 3) + ((board.Cols - 1) * 3);
                Console.WriteLine(new string('-', width));
            }
        }
    }
    public void ShowMessage(string message)
    {
        // General purpose message to the user
        Console.WriteLine(message);
    }
    public void ShowResult(string result)
    {
        // Display end-of-game result to the user, e.g., winner announcement or if there's a tie
        Console.WriteLine(result);
    }
    public void ShowHelp(string helpText)
    {
        // Display the help menu to the user
        Console.WriteLine(helpText);
    }
    public string GetInput(string prompt)
    {
        // Method shows a prompt and return the user's input
        Console.Write(prompt); // User types on the same line
        return Console.ReadLine();
    }

}
