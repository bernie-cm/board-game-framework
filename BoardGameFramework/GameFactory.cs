using BoardGameFramework.Core;
using BoardGameFramework.Commands;
namespace BoardGameFramework.Games;

// Responsible for creating fully configured Game instances
// Abstracts away the details of wiring up the board, history manager, saver, and players
// so the controller only needs to supply a game type and mode
public class GameFactory {
    // Creates a game of the given type with players configured according to the mode
    // mode is "hvh" (human vs human), "hvc" (human vs computer), or "cvh" (computer vs human)
    // Each case creates its own HistoryManager and GameSaver so games are fully independent
    public Game CreateGame(string type, string mode, IDisplay display) {
        var historyManager = new HistoryManager();
        var gameSaver = new GameSaver(historyManager);

        switch (type.ToLower()) {
            case "1": case "ntt":
                {
                    var game = new NumericalTicTacToeGame(display, historyManager, gameSaver);
                    var (nttP1, nttP2) = CreatePlayers(mode, "Odd", "Even",
                        new NTTComputerStrategy(), new NTTComputerStrategy());
                    game.SetPlayers(nttP1, nttP2);
                    return game;
                }
            case "2": case "notakto":
                {
                    var game = new NotaktoGame(display, historyManager, gameSaver);
                    var (notP1, notP2) = CreatePlayers(mode, "X", "X",
                        new NotaktoComputerStrategy(), new NotaktoComputerStrategy());
                    game.SetPlayers(notP1, notP2);
                    return game;
                }
            case "3": case "gomoku":
                {
                    var game = new GomokuGame(display, historyManager, gameSaver);
                    var (gomP1, gomP2) = CreatePlayers(mode, "X", "O",
                        new GomokuComputerStrategy(), new GomokuComputerStrategy());
                    game.SetPlayers(gomP1, gomP2);
                    return game;
                }
            default:
                throw new ArgumentException($"Unknown game type: '{type}'.");
        }
    }
    // Builds two players using the given pieces and strategies based on the selected mode.
    // Piece strings and strategy instances are supplied by the caller so this method
    // stays generic across all game types.
    private (Player p1, Player p2) CreatePlayers(string mode,
        string piece1, string piece2,
        IComputerStrategy strategy1, IComputerStrategy strategy2) {
        Player p1 = mode.ToLower() == "cvh"
            ? new ComputerPlayer(1, piece1, strategy1)
            : new HumanPlayer(1, piece1);

        Player p2 = mode.ToLower() == "hvc"
            ? new ComputerPlayer(2, piece2, strategy2)
            : new HumanPlayer(2, piece2);
        return (p1, p2);
    }
}