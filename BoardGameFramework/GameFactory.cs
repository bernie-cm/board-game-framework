namespace BoardGameFramework;

public class GameFactory
{
    
    public Game CreateGame(string type, IDisplay display)
    {
        var historyManager = new HistoryManager();
        var gameSaver = new GameSaver(historyManager);

        return type.ToLower() switch
        {
            "1" => new NumericalTicTacToeGame(display, historyManager, gameSaver),
            _ => throw new ArgumentException(
                     $"Unknown game type: '{type}'. Supported types: ntt")
        };
    }

    public Game CreateNewGame(string type, IDisplay display,
        Action<Game> configurePlayers)
    {
        var game = CreateGame(type, display);
        configurePlayers(game);
        return game;
    }

    private void ConfigurePlayers(string type, IDisplay display, bool vsComputer)
    {
        throw new NotImplementedException("ConfigurePlayers must be implemented per game type.\nUse CreateNewGame() with a configuration callback instead.");
    }
}
