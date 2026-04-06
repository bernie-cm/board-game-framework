using BoardGameFramework.Core;

namespace BoardGameFramework.Commands;

// Represents a single placement on the board as a command object.
// Storing the previous cell value means Undo can restore the exact state before the move,
// which is important for correctly reversing moves during an undo/redo sequence.
public class MoveCommand : ICommand
{
    private readonly Board _board;
    private readonly int _row;
    private readonly int _col;
    private readonly string _value;
    // Captured at execute time so Undo knows what to put back — almost always null (empty cell)
    private string? _previousValue;

    // Exposed so HistoryManager can serialise the stacks to save data
    public int Row => _row;
    public int Col => _col;
    public string Value => _value;

    public MoveCommand(Board board, int row, int col, string value)
    {
        _board = board;
        _row = row;
        _col = col;
        _value = value;
    }

    // Snapshots the current cell before placing so it can be restored by Undo
    public void Execute()
    {
        _previousValue = _board.GetCell(_row, _col);
        _board.PlaceMove(_row, _col, _value);
    }

    // Clears the cell and restores whatever was there before Execute was called
    public void Undo()
    {
        _board.RevertMove(_row, _col);
        if (_previousValue != null)
            _board.PlaceMove(_row, _col, _previousValue);
    }
}
