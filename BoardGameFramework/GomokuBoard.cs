using BoardGameFramework.Core;

namespace BoardGameFramework.Games;

// The board for Gomoku. Players take turns placing stones on a 15x15 grid.
// The first player to form an unbroken line of exactly five stones in any direction wins.
public class GomokuBoard : Board
{
    public GomokuBoard() : base(15, 15) { }

    // Scans all cells and directions for a run of five matching pieces.
    // Only starts a check from each cell to avoid counting the same line twice.
    public override bool CheckWin()
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
            {
                string? piece = GetCell(r, c);
                if (piece == null) continue;
                if (HasFiveInDirection(r, c, piece, 0, 1)) return true;   // horizontal →
                if (HasFiveInDirection(r, c, piece, 1, 0)) return true;   // vertical ↓
                if (HasFiveInDirection(r, c, piece, 1, 1)) return true;   // diagonal ↘
                if (HasFiveInDirection(r, c, piece, 1, -1)) return true;  // diagonal ↙
            }
        return false;
    }

    // Returns true when every cell has been filled — used to detect a draw
    public bool IsFull()
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                if (IsCellEmpty(r, c)) return false;
        return true;
    }

    // Counts how many consecutive matching pieces follow (startRow, startCol) in the given direction.
    // Used by the AI to measure how long a sequence would become if it placed at a given cell.
    public int CountInDirection(int startRow, int startCol, string piece, int dRow, int dCol)
    {
        int count = 0;
        int r = startRow + dRow, c = startCol + dCol;
        while (r >= 0 && r < Rows && c >= 0 && c < Cols && GetCell(r, c) == piece)
        {
            count++;
            r += dRow;
            c += dCol;
        }
        return count;
    }

    // Checks whether five cells in a row starting at (startRow, startCol) all contain the same piece
    private bool HasFiveInDirection(int startRow, int startCol, string piece, int dRow, int dCol)
    {
        int r = startRow, c = startCol;
        for (int i = 0; i < 5; i++)
        {
            if (r < 0 || r >= Rows || c < 0 || c >= Cols) return false;
            if (GetCell(r, c) != piece) return false;
            r += dRow;
            c += dCol;
        }
        return true;
    }
}
