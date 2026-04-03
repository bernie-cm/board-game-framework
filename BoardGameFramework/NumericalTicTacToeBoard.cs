namespace BoardGameFramework;

public class NumericalTicTacToeBoard : Board
{
    // Tracks which numbers have already been placed so duplicates are rejected.
    private readonly HashSet<int> _usedNumbers = new HashSet<int>();

    public NumericalTicTacToeBoard() : base(3, 3) { }

    public IEnumerable<int> GetAvailableNumbers(bool oddOnly)
    {
        var pool = oddOnly
            ? new[] { 1, 3, 5, 7, 9 }
            : new[] { 2, 4, 6, 8 };

        foreach (int n in pool)
            if (!_usedNumbers.Contains(n))
                yield return n;
    }

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

    public void PlaceNTTMove(int row, int col, int number)
    {
        PlaceMove(row, col, number.ToString());
        _usedNumbers.Add(number);
    }

    public void RevertNTTMove(int row, int col, int number)
    {
        RevertMove(row, col);
        _usedNumbers.Remove(number);
    }
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
    
    public bool IsFull()
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                if (IsCellEmpty(r, c)) return false;
        return true;
    }

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
