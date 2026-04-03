namespace BoardGameFramework;

public interface ICommand
{
    void Execute();
    void Undo();
}