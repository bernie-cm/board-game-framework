namespace BoardGameFramework;

/// <summary>
/// Factory responsible for creating and configuring Game instances.
/// Register new game types in the switch expression inside CreateGame().
/// </summary>
public class GameFactory
{
    /// <summary>
    /// Creates a fully wired-up game by type name.
    /// Recognised types: "ntt" (Numerical Tic-Tac-Toe).
    /// </summary>
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

    /// <summary>
    /// Creates a game and then runs a caller-supplied configuration callback
    /// (e.g. to swap in computer players before InitialiseGame is called).
    /// </summary>
    public Game CreateNewGame(string type, IDisplay display,
        Action<Game> configurePlayers)
    {
        var game = CreateGame(type, display);
        configurePlayers(game);
        return game;
    }

    /// <summary>
    /// Builds the player list for a game and adds them directly to the game's
    /// players collection via reflection on the protected field, or more simply
    /// by calling InitialiseGame and then replacing players.
    ///
    /// vsComputer: true  → Player 1 is human, Player 2 is ComputerMoveLogic
    ///             false → both players are human
    ///
    /// The board reference needed by ComputerMoveLogic is supplied after
    /// InitialiseGame() has been called, so SetContext() is used to wire it up.
    /// Concrete games that want computer support should call this after
    /// constructing the game but before PlayGame().
    /// </summary>
    private void ConfigurePlayers(string type, IDisplay display, bool vsComputer)
    {
        // Player configuration is game-specific and applied via CreateNewGame's
        // callback. This private method documents the intended signature from
        // the class diagram and is the hook point for concrete implementations.
        throw new NotImplementedException(
            "ConfigurePlayers must be implemented per game type. " +
            "Use CreateNewGame() with a configuration callback instead.");
    }
}
