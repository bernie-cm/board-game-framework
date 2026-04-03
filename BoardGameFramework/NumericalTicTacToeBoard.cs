namespace BoardGameFramework;

/// <summary>
/// A 3x3 board for Numerical Tic-Tac-Toe.
///
/// Rules:
///   - Player 1 owns the ODD numbers  {1, 3, 5, 7, 9}
///   - Player 2 owns the EVEN numbers {2, 4, 6, 8}
///   - Each number may only be placed once across the whole game.
///   - A player wins when any row, column or diagonal sums to exactly 15.
///   - The board is full (draw) when all 9 cells are occupied with no winner.
/// </summary>
public class NumericalTicTacToeBoard : Board
{
    // Tracks which numbers have already been placed so duplicates are rejected.
    private readonly HashSet<int> _usedNumbers = new HashSet<int>();

    public NumericalTicTacToeBoard() : base(3, 3) { }

    /// <summary>
    /// Returns the set of numbers that have not yet been placed on the board.
    /// </summary>
    public IEnumerable<int> GetAvailableNumbers(bool oddOnly)
    {
        var pool = oddOnly
            ? new[] { 1, 3, 5, 7, 9 }
            : new[] { 2, 4, 6, 8 };

        foreach (int n in pool)
            if (!_usedNumbers.Contains(n))
                yield return n;
    }

    /// <summary>
    /// Extended validation: cell must be empty AND the number must not have been
    /// used already AND the number must belong to the correct player pool.
    /// </summary>
    public bool IsValidNTTMove(int row, int col, int number, bool isOddPlayer)
    {
        // Cell must be in bounds and empty
        if (!IsValidMove(row, col)) return false;

        // Number must not have been placed before
        if (_usedNumbers.Contains(number)) return false;

        // Number must belong to the correct player's pool
        bool numberIsOdd = number % 2 != 0;
        return numberIsOdd == isOddPlayer;
    }

    /// <summary>
    /// Places the number on the board and records it as used.
    /// Overrides the base so the used-set is always kept in sync.
    /// </summary>
    public void PlaceNTTMove(int row, int col, int number)
    {
        PlaceMove(row, col, number.ToString());
        _usedNumbers.Add(number);
    }

    /// <summary>
    /// Reverts a cell and removes the number from the used set (for undo).
    /// </summary>
    public void RevertNTTMove(int row, int col, int number)
    {
        RevertMove(row, col);
        _usedNumbers.Remove(number);
    }

    /// <summary>
    /// Checks whether any row, column, or diagonal currently sums to 15.
    /// Empty cells count as 0 so they never accidentally complete a line.
    /// </summary>
    public override bool CheckWin()
    {
        // Check all rows
        for (int r = 0; r < Rows; r++)
            if (LineSum(r, 0, 0, 1) == 15) return true;

        // Check all columns
        for (int c = 0; c < Cols; c++)
            if (LineSum(0, c, 1, 0) == 15) return true;

        // Check main diagonal (top-left → bottom-right)
        if (LineSum(0, 0, 1, 1) == 15) return true;

        // Check anti-diagonal (top-right → bottom-left)
        if (LineSum(0, 2, 1, -1) == 15) return true;

        return false;
    }

    /// <summary>
    /// Returns true when every cell is filled (used to detect a draw).
    /// </summary>
    public bool IsFull()
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                if (IsCellEmpty(r, c)) return false;
        return true;
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Sums the three cells of a line starting at (startRow, startCol) and
    /// stepping by (dRow, dCol) for the length of the board.
    /// </summary>
    private int LineSum(int startRow, int startCol, int dRow, int dCol)
    {
        int sum = 0;
        int r = startRow, c = startCol;
        for (int i = 0; i < 3; i++)
        {
            string? cell = GetCell(r, c);
            if (cell != null && int.TryParse(cell, out int val))
                sum += val;
            r += dRow;
            c += dCol;
        }
        return sum;
    }
}
