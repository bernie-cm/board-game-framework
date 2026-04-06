using System.Text.Json;
using BoardGameFramework.Commands;
using BoardGameFramework.Games;
using BoardGameUI;

namespace BoardGameFramework.Core;

// Handles reading and writing game state to disk as JSON.
// Separates persistence concerns from game logic so individual game classes
// only need to implement ToSaveData and RestoreFromSaveData.
public class GameSaver
{
    private readonly HistoryManager _historyManager;

    public GameSaver(HistoryManager historyManager)
    {
        _historyManager = historyManager;
    }

    // Serialises the current game state to a JSON file at the given path
    public void SaveGame(Game game, string filePath)
    {
        var data = game.ToSaveData();
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    // Returns the raw SaveData without constructing a Game — used by Game.PlayGame for in-game loads
    public SaveData LoadGameData(string filePath)
    {
        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<SaveData>(json)
            ?? throw new InvalidDataException("Could not deserialise save file.");
    }

    // Constructs and returns a fully restored Game — used by GameController for menu-level loads.
    // Marks the game so PlayGame skips InitialiseGame and resumes from the saved state.
    public Game LoadGame(string filePath)
    {
        var data = LoadGameData(filePath);
        var game = data.RestoreGame(_historyManager);
        game.MarkRestoredFromLoad();
        return game;
    }
}

// Holds a single move's position and value so undo/redo stacks can be serialised to JSON
public class MoveRecord
{
    public int Row { get; set; }
    public int Col { get; set; }
    public string Value { get; set; } = string.Empty;
}

// The full snapshot of a game at the point it was saved.
// Stores the board grid, player list, whose turn it is, and the undo/redo stacks
// so the game can be restored exactly as it was left.
public class SaveData
{
    public string GameType { get; set; } = string.Empty;
    public int Rows { get; set; }
    public int Cols { get; set; }
    public List<List<string?>>? Grid { get; set; }
    public List<PlayerData> Players { get; set; } = new();
    public int CurrentPlayerIndex { get; set; }
    public List<MoveRecord> UndoStack { get; set; } = new();
    public List<MoveRecord> RedoStack { get; set; } = new();

    // Constructs the correct game subclass based on GameType and calls RestoreFromSaveData on it
    public Game RestoreGame(HistoryManager historyManager)
    {
        var display = new ConsoleDisplay();
        var gameSaver = new GameSaver(historyManager);

        switch (GameType.ToLower())
        {
            case "ntt":
                {
                    var game = new NumericalTicTacToeGame(display, historyManager, gameSaver);
                    game.RestoreFromSaveData(this);
                    return game;
                }
            case "notakto":
                {
                    var game = new NotaktoGame(display, historyManager, gameSaver);
                    game.RestoreFromSaveData(this);
                    return game;
                }
            case "gomoku":
                {
                    var game = new GomokuGame(display, historyManager, gameSaver);
                    game.RestoreFromSaveData(this);
                    return game;
                }
            default:
                throw new NotSupportedException(
                    $"Cannot restore unknown game type '{GameType}'.");
        }
    }
}

// Stores the data needed to reconstruct a player — their number, piece, and whether they are human.
// IsHuman is used on load to decide whether to create a HumanPlayer or a ComputerPlayer.
public class PlayerData
{
    public int PlayerNumber { get; set; }
    public string GamePiece { get; set; } = string.Empty;
    public bool IsHuman { get; set; }
}
