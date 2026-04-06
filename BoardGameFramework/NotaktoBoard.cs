using BoardGameFramework.Core;

namespace BoardGameFramework.Games;

// The board for 3-board Notakto.
// Standard Notakto uses three independent 3×3 grids rather than one.
// Players alternate placing X on any cell of any live (un-dead) board each turn.
// A board dies when it contains a complete line of three X's.
// The player who places the X that kills the last surviving board loses.
//
// Internally, the three boards are stacked into a single 9×3 grid so the
// rest of the framework (MoveCommand, HistoryManager, SaveData) needs no changes:
//   rows 0–2  → board 1  (boardIndex 0)
//   rows 3–5  → board 2  (boardIndex 1)
//   rows 6–8  → board 3  (boardIndex 2)
public class NotaktoBoard : Board
{
    // Tracks which of the three boards has been killed by a completed line.
    // Once a board is dead, no further moves can be placed on it.
    private readonly bool[] _boardDead = new bool[3];

    // 3 boards × 3 rows each = 9 logical rows in the underlying grid
    public NotaktoBoard() : base(9, 3) { }

    public int BoardCount => 3;

    // Returns true if the given board has been killed by a completed line
    public bool IsBoardDead(int boardIndex) => _boardDead[boardIndex];

    // Converts a (boardIndex, localRow) pair to the global row used by the base grid.
    // Callers outside this class use this to build MoveCommand coordinates.
    public static int GlobalRow(int boardIndex, int localRow) => boardIndex * 3 + localRow;

    // Returns true only if the target board is still alive and the cell is empty.
    // Replaces IsValidMove for all Notakto-specific move validation.
    public bool IsValidNotaktoMove(int boardIndex, int localRow, int col)
        => !_boardDead[boardIndex] && IsValidMove(GlobalRow(boardIndex, localRow), col);

    // After placing, checks whether that board just acquired a complete line.
    // If so, marks it dead immediately so no further moves are accepted on it.
    public override void PlaceMove(int row, int col, string value)
    {
        base.PlaceMove(row, col, value);
        int boardIndex = row / 3;
        if (CheckBoardForLine(boardIndex))
            _boardDead[boardIndex] = true;
    }

    // When a move is undone, the affected board's death status must be re-evaluated
    // because removing the piece might have eliminated the only complete line on it.
    public override void RevertMove(int row, int col)
    {
        base.RevertMove(row, col);
        int boardIndex = row / 3;
        _boardDead[boardIndex] = CheckBoardForLine(boardIndex);
    }

    // Returns true when all three boards are dead.
    // This is the game-ending condition: the player who placed the last X loses.
    public override bool CheckWin()
        => _boardDead[0] && _boardDead[1] && _boardDead[2];

    // Returns true when every cell across all three boards has been filled.
    // In practice this coincides with CheckWin (a full board always contains a line),
    // but it is kept as a safety net for the draw check in EndOfGame.
    public bool IsFull()
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                if (IsCellEmpty(r, c)) return false;
        return true;
    }

    // Returns true if placing X at (boardIndex, localRow, col) would complete a line
    // on that board — i.e., kill it. Used by the AI to identify losing moves.
    // Only call this on cells that pass IsValidNotaktoMove to avoid side effects.
    public bool WouldKillBoard(int boardIndex, int localRow, int col)
    {
        int globalRow = GlobalRow(boardIndex, localRow);
        PlaceMove(globalRow, col, "X");
        bool kills = CheckBoardForLine(boardIndex);
        RevertMove(globalRow, col);
        return kills;
    }

    // Returns a formatted string showing the three boards side-by-side, suitable for
    // passing directly to IDisplay.ShowMessage. Using ShowMessage instead of ShowBoard
    // lets us arrange the boards horizontally, which is much easier to read than a 9-row stack.
    public string GetDisplayString()
    {
        var lines = new System.Text.StringBuilder();
        lines.AppendLine();

        // Header row: board numbers and dead status.
        // Each label must be exactly 11 chars wide to line up with the 11-char board rows below.
        // Board row width: " . " + "|" + " . " + "|" + " . " = 3+1+3+1+3 = 11 chars.
        for (int b = 0; b < 3; b++)
        {
            string label = (_boardDead[b] ? $"Board {b + 1} [X]" : $"  Board {b + 1}  ").PadRight(11);
            lines.Append(label);
            if (b < 2) lines.Append("   ");
        }
        lines.AppendLine();

        // Three rows of cells, one row at a time, across all boards side-by-side
        for (int localRow = 0; localRow < 3; localRow++)
        {
            for (int b = 0; b < 3; b++)
            {
                int globalRow = GlobalRow(b, localRow);
                for (int col = 0; col < 3; col++)
                {
                    string cell = IsCellEmpty(globalRow, col) ? "." : GetCell(globalRow, col)!;
                    lines.Append($" {cell} ");
                    if (col < 2) lines.Append("|");
                }
                if (b < 2) lines.Append("   ");
            }
            lines.AppendLine();

            // Separator between rows within each board (but not after the last row)
            if (localRow < 2)
            {
                for (int b = 0; b < 3; b++)
                {
                    lines.Append("-----------");
                    if (b < 2) lines.Append("   ");
                }
                lines.AppendLine();
            }
        }

        return lines.ToString();
    }

    // Returns true if the sub-board at the given boardIndex currently contains
    // any complete line of three X's — checked without modifying any state.
    private bool CheckBoardForLine(int boardIndex)
    {
        int baseRow = boardIndex * 3;
        for (int r = 0; r < 3; r++)
            if (IsFullLine(baseRow + r, 0, 0, 1)) return true;
        for (int c = 0; c < 3; c++)
            if (IsFullLine(baseRow, c, 1, 0)) return true;
        if (IsFullLine(baseRow, 0, 1, 1)) return true;
        if (IsFullLine(baseRow, 2, 1, -1)) return true;
        return false;
    }

    // Checks whether three consecutive cells in the given direction are all X
    private bool IsFullLine(int startRow, int startCol, int dRow, int dCol)
    {
        int r = startRow, c = startCol;
        for (int i = 0; i < 3; i++)
        {
            if (GetCell(r, c) != "X") return false;
            r += dRow;
            c += dCol;
        }
        return true;
    }
}
