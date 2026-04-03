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
        currentPlayer = players[(players.IndexOf(currentPlayer) + 1) % players.Count];
    }
    public void UndoMove()
    {
        //First check if undo is possible, then call historyManager to undo the last move
        if (historyManager.CanUndo())
        {
            historyManager.Undo();
            SwitchPlayer(); //Return back to the previous player
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
            SwitchPlayer(); //Go forward to the next player
        } else
        {
            display.ShowMessage("No moves to redo.");
        }
    }
    public void SaveGame(string filePath)
    {
        //tells gameSaver to save the current game state to a file located in filePath. filepath can be pretty much anyting at the moment.
        gameSaver.SaveGame(this, filePath);
        display.ShowMessage("Your game has been saved successfully.");
    }
    public void LoadGame(string filePath)
    {
        //gets from gameSaver. Restores board, players, history.
        gameSaver.LoadGame(filePath);
        display.ShowMessage("Your game has been loaded successfully.");
    }

    public virtual SaveData ToSaveData()
    {
        throw new NotImplementedException("ToSaveData() must be overridden in game classes");
    }
}