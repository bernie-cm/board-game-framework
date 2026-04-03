using System.Drawing;

namespace BoardGameFramework;

public class ConsoleDisplay : IDisplay
{
    public void ShowBoard(Board board)
    {
        Console.WriteLine();
        for (int row = 0; row < board.Rows; row++)
        {
            for (int col = 0; col < board.Cols; col++)
            {
                if (col + 1 == board.Cols)
                {
                    if (board.GetCell(row, col) == null) Console.Write("  ");
                    else Console.Write(" " + board.GetCell(row, col));
                }
                else
                {
                    if (board.GetCell(row, col) == null) Console.Write("  ");
                    else Console.Write(" " + board.GetCell(row, col));
                    Console.Write(" |");
                }
            }
            Console.WriteLine();
            if (row + 1 != board.Cols)
            {
                for (int i = 0; i < (4 * board.Cols); i++) Console.Write("_");
                Console.WriteLine();
                for (int i = 0; i < (4 * board.Cols); i++) Console.Write(" ");
                Console.WriteLine();
            }
        }
        Console.WriteLine();
    }
    public void ShowMessage(string message)
    {
        Console.WriteLine(message);
    }

    public void ShowResult(string result)
    {
        Console.WriteLine($"\n=== {result} ===");
    }

    public void ShowHelp(string help)
    {
        Console.WriteLine($"\n[Help] {help}");
    }

    public string GetInput(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? string.Empty;
    }
}
