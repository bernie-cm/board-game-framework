using BoardGameFramework.Core;

namespace BoardGameFramework.Commands;

// Manages the undo and redo stacks for a game session using the Command pattern.
// Every executed move is pushed onto the undo stack. When a move is undone it moves
// to the redo stack, and when redone it moves back. Executing a new move clears the
// redo stack because the previous future is no longer valid.
public class HistoryManager
{
    private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
    private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();

    public bool CanUndo() => _undoStack.Count > 0;
    public bool CanRedo() => _redoStack.Count > 0;

    // Executes the command and pushes it onto the undo stack.
    // Clears the redo stack because branching from a previous state invalidates the saved future.
    public void Execute(ICommand command)
    {
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();
    }

    // Pops the most recent command off the undo stack, reverses it, and saves it for redo
    public void Undo()
    {
        if (!CanUndo()) return;
        var command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);
    }

    // Pops the most recently undone command off the redo stack and re-executes it
    public void Redo()
    {
        if (!CanRedo()) return;
        var command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);
    }

    // Wipes both stacks — called at the start of a new game to ensure there is no leftover history
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }

    public IEnumerable<ICommand> GetUndoHistory() => _undoStack;

    // Returns the undo stack as move records (top → bottom order) for serialisation
    public List<MoveRecord> GetUndoStack() => _undoStack
        .Cast<MoveCommand>()
        .Select(c => new MoveRecord { Row = c.Row, Col = c.Col, Value = c.Value })
        .ToList();

    // Returns the redo stack as move records (top → bottom order) for serialisation
    public List<MoveRecord> GetRedoStack() => _redoStack
        .Cast<MoveCommand>()
        .Select(c => new MoveRecord { Row = c.Row, Col = c.Col, Value = c.Value })
        .ToList();

    // Restores both stacks from saved records so undo/redo works correctly after loading a save file.
    // Records are pushed in reverse so the first record in the list ends up on top of the stack,
    // matching the order the moves were originally made in.
    public void RestoreStacks(List<MoveRecord> undoRecords, List<MoveRecord> redoRecords, Board board)
    {
        _undoStack.Clear();
        _redoStack.Clear();
        foreach (var r in Enumerable.Reverse(undoRecords))
            _undoStack.Push(new MoveCommand(board, r.Row, r.Col, r.Value));
        foreach (var r in Enumerable.Reverse(redoRecords))
            _redoStack.Push(new MoveCommand(board, r.Row, r.Col, r.Value));
    }
}
