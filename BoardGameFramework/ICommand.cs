namespace BoardGameFramework.Core;

// Defines the contract for all game actions that can be undone and redone.
// Every move in the game is represented as an ICommand so the HistoryManager
// can store, reverse, and replay them without knowing what the move actually does.
public interface ICommand
{
    // Applies the action to the board
    void Execute();
    // Reverses the action, restoring the board to how it was before Execute was called
    void Undo();
}
