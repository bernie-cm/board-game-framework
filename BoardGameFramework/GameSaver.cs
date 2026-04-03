using System.Text.Json;
namespace BoardGameFramework;

public class GameSaver
{
    private readonly HistoryManager _historyManager;

    public GameSaver(HistoryManager historyManager)
    {
        _historyManager = historyManager;
    }

    public void SaveGame(Game game, string filePath)
    {
        var data = game.ToSaveData();
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    public Game LoadGame(string filePath)
    {
        string json = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize<SaveData>(json) ?? throw new InvalidDataException("Could not deserialise save file.");
        return data.RestoreGame(_historyManager);
    }
}

public class SaveData
{
    public string GameType { get; set; } = string.Empty;
    public int Rows { get; set; }
    public int Cols { get; set; }
    public List<List<string?>>? Grid { get; set; }
    public List<PlayerData> Players { get; set; } = new();
    public int CurrentPlayerIndex { get; set; }

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
            default:
                throw new NotSupportedException(
                    $"Cannot restore unknown game type '{GameType}'.");
        }
    }
}

public class PlayerData
{
    public int PlayerNumber { get; set; }
    public string GamePiece { get; set; } = string.Empty;
    public bool IsHuman { get; set; }
}
