namespace BoardGameFramework;

/// <summary>
/// Command pattern: encapsulates a single board move with Execute/Undo support.
/// </summary>
public class MoveCommand : ICommand
{
    private readonly Board _board;
    private readonly int _row;
    private readonly int _col;
    private readonly string _value;
    private string? _previousValue;

    public MoveCommand(Board board, int row, int col, string value)
    {
        _board = board;
        _row = row;
        _col = col;
        _value = value;
    }

    public void Execute()
    {
        _previousValue = _board.GetCell(_row, _col);
        _board.PlaceMove(_row, _col, _value);
    }

    public void Undo()
    {
        _board.RevertMove(_row, _col);
        if (_previousValue != null)
            _board.PlaceMove(_row, _col, _previousValue);
    }
}
