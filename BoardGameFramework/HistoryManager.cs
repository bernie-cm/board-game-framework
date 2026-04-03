namespace BoardGameFramework;

public class HistoryManager
{
    private readonly Stack<ICommand> undoStack = new Stack<ICommand>();
    private readonly Stack<ICommand> redoStack = new Stack<ICommand>();

    public bool CanUndo() => undoStack.Count > 0;
    public bool CanRedo() => redoStack.Count > 0;
    
    //pushes new command to stack
    public void Execute(ICommand command)
    {
        command.Execute();
        undoStack.Push(command);
        redoStack.Clear(); 
    }

    public void Undo()
    {
        if (!CanUndo()) return;
        var command = undoStack.Pop();
        command.Undo();
        redoStack.Push(command);
    }

    public void Redo()
    {
        if (!CanRedo()) return;
        var command = redoStack.Pop();
        command.Execute();
        undoStack.Push(command);
    }

    public void Clear()
    {
        undoStack.Clear();
        redoStack.Clear();
    }

    public IEnumerable<ICommand> GetUndoHistory() => undoStack;
}