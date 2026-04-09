using BoardGameFramework.Commands;
namespace BoardGameFramework.Core;

// Abstract base class for all games in the framework.
// Provides the core game loop, undo/redo, save/load, and in-game command signalling so individual game classes only need to implement their own rules and board logic.
public abstract class Game {
    protected Board _board;
    protected Player _currentPlayer;
    protected List<Player> _players = new List<Player>();
    protected IDisplay _display;
    protected HistoryManager _historyManager;
    protected GameSaver _gameSaver;
    // Used by in-game commands to signal a load or exit to the PlayGame loop.
    // Keeping these as flags means MakePlay can set them and return cleanly
    // rather than needing to throw an exception or return a special value.
    private string? _pendingLoadPath = null;
    private bool _exitRequested = false;
    // Set by RestoreFromSaveData so that PlayGame skips InitialiseGame when resuming a loaded game.
    // Without this, PlayGame would reset the board that RestoreFromSaveData just populated.
    private bool _skipInit = false;
    protected Game(IDisplay display, HistoryManager historyManager, GameSaver gameSaver) {
        _display = display;
        _historyManager = historyManager;
        _gameSaver = gameSaver;
    }
    // Main game loop. Runs until the game ends, the player exits, or a load is requested.
    // After each turn it checks whether MakePlay signalled a load or exit before advancing
    // to the next player. On a successful load, currentIndex is re-synced to the loaded state.
    public void PlayGame() {
        if (_skipInit)
            _skipInit = false;
        else
            InitialiseGame();
        int currentIndex = _players.Count > 0 ? _players.IndexOf(_currentPlayer) : 0;
        while (!EndOfGame() && !_exitRequested) {
            MakePlay(currentIndex);
            if (_exitRequested) break;
            if (_pendingLoadPath != null) {
                string path = _pendingLoadPath;
                _pendingLoadPath = null;
                try {
                    var data = _gameSaver.LoadGameData(path);
                    RestoreFromSaveData(data);
                    currentIndex = _players.IndexOf(_currentPlayer);
                    _display.ShowMessage("Game loaded successfully.");
                }
                catch (Exception ex) {
                    _display.ShowMessage($"Failed to load game: {ex.Message}");
                }
                continue;
            }
            currentIndex = (currentIndex + 1) % _players.Count;
        }
        if (!_exitRequested)
            PrintWinner();
    }
    // Sets up a fresh game — creates the board, clears history, and sets the starting player
    protected abstract void InitialiseGame();
    // Returns true when the game has reached an end condition (win or draw)
    protected abstract bool EndOfGame();
    // Handles one player's full turn, including reading input and executing their move
    protected abstract void MakePlay(int player);
    // Displays the final board state and announces the result
    protected abstract void PrintWinner();
    // Advances _currentPlayer to the next player in the list, wrapping around at the end
    protected void SwitchPlayer() {
        _currentPlayer = _players[(_players.IndexOf(_currentPlayer) + 1) % _players.Count];
    }
    // Delegates to HistoryManager to reverse the last move, then switches back to the previous player
    public void UndoMove() {
        if (_historyManager.CanUndo()) {
            _historyManager.Undo();
            SwitchPlayer();
        } else {
            _display.ShowMessage("No moves to undo.");
        }
    }
    // Delegates to HistoryManager to reapply the last undone move, then advances to the next player
    public void RedoMove() {
        if (_historyManager.CanRedo()) {
            _historyManager.Redo();
            SwitchPlayer();
        } else {
            _display.ShowMessage("No moves to redo.");
        }
    }
    // Serialises the current game state to disk via GameSaver
    public void SaveGame(string filePath) {
        _gameSaver.SaveGame(this, filePath);
        _display.ShowMessage("Your game has been saved successfully.");
    }
    // Loads save data from disk and restores the current game's state in-place
    public void LoadGame(string filePath) {
        var data = _gameSaver.LoadGameData(filePath);
        RestoreFromSaveData(data);
        _display.ShowMessage("Your game has been loaded successfully.");
    }
    // Called from MakePlay to defer a load until the end of the current turn.
    // Setting a flag rather than loading immediately keeps MakePlay simple and avoids
    // mutating game state mid-turn while the command loop is still running.
    protected void RequestLoad(string filePath) {
        _pendingLoadPath = filePath;
    }
    // Called from MakePlay to signal that the player wants to quit to the main menu
    protected void RequestExit() {
        _exitRequested = true;
    }
    // Overridden by each game class to reconstruct board, players, and history from a save file
    public virtual void RestoreFromSaveData(SaveData data) {}
    // Called by GameSaver.LoadGame after RestoreFromSaveData so that the next PlayGame call
    // resumes from the restored state rather than resetting the board via InitialiseGame
    public void MarkRestoredFromLoad() { _skipInit = true; }
    // Each game class must implement this to describe its full state as a SaveData object
    public abstract SaveData ToSaveData();
}