namespace BoardGameFramework;

public abstract class Game {
    protected Board board;
    protected Player currentPlayer;
    protected List<Player> players = new List<Player>();
    protected IDisplay display;
    protected HistoryManager historyManager;
    protected GameSaver gameSaver;

    protected Game(IDisplay display, HistoryManager historyManager, GameSaver gameSaver)
    {
        this.display = display;
        this.historyManager = historyManager;
        this.gameSaver = gameSaver;
    }
    public void PlayGame()
    {
        // Template method
        InitialiseGame();
        int currentIndex = 0;
        while (!EndOfGame())
        {
            MakePlay(currentIndex);
            currentIndex = (currentIndex + 1) % players.Count;
        }
        PrintWinner();
    }
    protected abstract void InitialiseGame();
    protected abstract bool EndOfGame();
    protected abstract void MakePlay(int player);
    protected abstract void PrintWinner();
    protected void SwitchPlayer()
    {
        // Sets currentPlayer to the next in the list
        // players.IndexOf(currentPlayer) finds the position of the current player in the list (Player 1 is index 0 and Player 2 is index 1)
        // Adding 1 moves to the next player, so index 0 becomes 1, and index 1 becomes 2
        // The module is to wrap around the list. When there are two players, players.Count is 2
        // so Player at index 0: (0 + 1) % 2 = 1 and this switches to Player 2
        // Player at index 1: (1 + 1) % 2 = 0 and this wraps back to Player 1
        currentPlayer = players[(players.IndexOf(currentPlayer) + 1) % players.Count];
    }
    public void UndoMove()
    {
        //First check if undo is possible, then call historyManager to undo the last move
        if (historyManager.CanUndo())
        {
            historyManager.Undo();
            SwitchPlayer(); // Return back to the previous player
        } else
        {
            display.ShowMessage("No moves to undo.");
        }
    }
    public void RedoMove()
    {
        //First check if redo is possible, then call historyManager to redo the last move
        if (historyManager.CanRedo())
        {
            historyManager.Redo();
            SwitchPlayer(); // Go forward to the next player
        } else
        {
            display.ShowMessage("No moves to redo.");
        }
    }
    public void SaveGame(string filePath)
    {
        //Delegates to gameSaver to save the current game state to a file located in filePath
        gameSaver.SaveGame(this, filePath);
        display.ShowMessage("Your game has been saved successfully.");
    }
    public void LoadGame(string filePath)
    {
        // Delegates to gameSaver. Restores board, players, history.
        gameSaver.LoadGame(filePath);
        display.ShowMessage("Your game has been loaded successfully.");
    }
}