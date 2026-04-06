using BoardGameFramework.Core;

namespace BoardGameFramework.Games;

// The board for Numerical Tic-Tac-Toe. Extends Board with NTT-specific rules:
// each number can only be placed once across the whole game, and players are
// restricted to odd or even numbers depending on which player they are.
// Supports any board size n ≥ 3 — the win target scales with the magic square
// formula n(n²+1)/2, and the number pools automatically cover 1 to n².
public class NumericalTicTacToeBoard : Board
{
    // Tracks which numbers have already been placed so duplicates are rejected
    private readonly HashSet<int> _usedNumbers = new HashSet<int>();

    // The sum any complete row, column, or diagonal must reach to win.
    // Derived from the board size: n(n²+1)/2.  For a 3×3 board this is 15,
    // for 4×4 it is 34, for 5×5 it is 65, and so on.
    public int TargetSum { get; }

    // Defaults to 3×3 so the rest of the framework can create a board
    // without knowing the size upfront (e.g. during deserialisation setup).
    public NumericalTicTacToeBoard(int size = 3) : base(size, size)
    {
        TargetSum = size * (size * size + 1) / 2;
    }

    // Returns only the numbers from the given player's pool that haven't been placed yet.
    // The full pool is all odd (or even) numbers from 1 to n² — generated on the fly
    // so this method works correctly for any board size without hard-coded arrays.
    public IEnumerable<int> GetAvailableNumbers(bool oddOnly)
    {
        int start = oddOnly ? 1 : 2;
        for (int n = start; n <= Rows * Cols; n += 2)
            if (!_usedNumbers.Contains(n))
                yield return n;
    }

    // Checks all three conditions a valid NTT move must satisfy:
    // the cell must be empty, the number must be in the valid range for this board size,
    // it must not have been used before, and it must belong to the current player's pool.
    public bool IsValidNTTMove(int row, int col, int number, bool isOddPlayer)
    {
        if (!IsValidMove(row, col)) return false;
        if (number < 1 || number > Rows * Cols) return false;
        if (_usedNumbers.Contains(number)) return false;
        bool numberIsOdd = number % 2 != 0;
        return numberIsOdd == isOddPlayer;
    }

    // Overrides PlaceMove to also register the number as used.
    // This ensures _usedNumbers stays in sync whether the move comes from
    // a MoveCommand, RestoreFromSaveData, or anywhere else.
    public override void PlaceMove(int row, int col, string value)
    {
        base.PlaceMove(row, col, value);
        if (int.TryParse(value, out int number))
            _usedNumbers.Add(number);
    }

    // Overrides RevertMove to also remove the number from the used set,
    // keeping _usedNumbers consistent when a move is undone
    public override void RevertMove(int row, int col)
    {
        string? cell = GetCell(row, col);
        if (cell != null && int.TryParse(cell, out int number))
            _usedNumbers.Remove(number);
        base.RevertMove(row, col);
    }

    // Convenience method for restoring board state from a save file using a typed int rather than a string
    public void PlaceNTTMove(int row, int col, int number)
    {
        PlaceMove(row, col, number.ToString());
    }

    // Checks every row, column, and diagonal to see if any line sums to the target.
    // Works for any board size because LineSum steps Rows times and uses TargetSum.
    public override bool CheckWin()
    {
        for (int r = 0; r < Rows; r++)
            if (LineSum(r, 0, 0, 1) == TargetSum) return true;

        for (int c = 0; c < Cols; c++)
            if (LineSum(0, c, 1, 0) == TargetSum) return true;

        // Main diagonal (top-left → bottom-right)
        if (LineSum(0, 0, 1, 1) == TargetSum) return true;

        // Anti-diagonal starts at top-right corner, which is column (n-1) for an n×n board
        if (LineSum(0, Cols - 1, 1, -1) == TargetSum) return true;

        return false;
    }

    // Returns true when every cell has been filled, signalling a draw if no winner has been found
    public bool IsFull()
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                if (IsCellEmpty(r, c)) return false;
        return true;
    }

    // Walks n cells in the direction given by (dRow, dCol) and returns their sum.
    // Using Rows as the step count (rather than the old hardcoded 3) makes this
    // correct for any square board size.
    private int LineSum(int startRow, int startCol, int dRow, int dCol)
    {
        int sum = 0;
        int r = startRow, c = startCol;
        for (int i = 0; i < Rows; i++)
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
