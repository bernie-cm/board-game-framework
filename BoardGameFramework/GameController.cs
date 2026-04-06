using BoardGameFramework.Core;
using BoardGameFramework.Commands;
using BoardGameFramework.Games;

namespace BoardGameUI;

// Entry point for the application's user interaction loop.
// Owns the main menu and delegates game creation and loading to GameFactory and GameSaver.
// No game logic lives here — it only navigates between screens and hands off to the game.
public class GameController
{
    private readonly GameFactory _gameFactory;
    private readonly IDisplay _display;

    public GameController(IDisplay display, GameFactory gameFactory)
    {
        _display = display;
        _gameFactory = gameFactory;
    }

    // Keeps showing the main menu until the player chooses to exit
    public void Start()
    {
        bool running = true;
        while (running)
        {
            running = ShowMainMenu();
        }
    }

    // Displays the main menu and routes the player's choice to the appropriate method.
    // Returns false only when the player selects Exit, which terminates the Start loop.
    private bool ShowMainMenu()
    {
        _display.ShowMessage("\n=== Board Game Framework ===");
        _display.ShowMessage("1. Numerical Tic-Tac-Toe");
        _display.ShowMessage("2. Notakto");
        _display.ShowMessage("3. Gomoku");
        _display.ShowMessage("4. Load Game");
        _display.ShowMessage("5. Help");
        _display.ShowMessage("6. Exit");

        string choice = _display.GetInput("Choose an option: ").Trim();
        switch (choice)
        {
            case "1":
            case "2":
            case "3":
                StartNewGame(choice);
                return true;
            case "4":
                LoadSavedGame();
                return true;
            case "5":
                ShowHelp();
                return true;
            case "6":
                _display.ShowMessage("Goodbye!");
                return false;
            default:
                _display.ShowMessage("Invalid option. Please choose 1-6.");
                return true;
        }
    }

    // Asks the player to pick a game mode before handing off to GameFactory.
    // If the player selects Back, an empty string is returned and the game is not started.
    private void StartNewGame(string gameType)
    {
        string mode = SelectGameMode();
        if (mode == "") return; // user chooses Back

        try
        {
            var game = _gameFactory.CreateGame(gameType, mode, _display);
            game.PlayGame();
        }
        catch (ArgumentException ex)
        {
            _display.ShowMessage($"Error: {ex.Message}");
        }
    }

    // Shows the mode selection screen and returns the chosen mode string,
    // or an empty string if the player chose to go back
    private string SelectGameMode()
    {
        _display.ShowMessage("\nSelect game mode:");
        _display.ShowMessage("1. Human vs Human");
        _display.ShowMessage("2. Human vs Computer");
        _display.ShowMessage("3. Computer vs Human");
        _display.ShowMessage("4. Back");

        string choice = _display.GetInput("Mode: ").Trim();
        return choice switch
        {
            "1" => "hvh",
            "2" => "hvc",
            "3" => "cvh",
            _   => ""
        };
    }

    // Prompts for a file path, then constructs a fresh Game from the save file and starts it.
    // A new HistoryManager and GameSaver are created here so the loaded game is self-contained.
    private void LoadSavedGame()
    {
        string path = _display.GetInput("Enter save file path: ").Trim();
        if (!File.Exists(path))
        {
            _display.ShowMessage("File not found.");
            return;
        }

        try
        {
            var historyManager = new HistoryManager();
            var gameSaver = new GameSaver(historyManager);
            var game = gameSaver.LoadGame(path);
            game.PlayGame();
        }
        catch (Exception ex)
        {
            _display.ShowMessage($"Failed to load game: {ex.Message}");
        }
    }

    // Displays a summary of available games and all in-game commands
    private void ShowHelp()
    {
        _display.ShowHelp(
            "Available games:\n" +
            "  1. Numerical Tic-Tac-Toe — place odd/even numbers; first line summing to 15 wins\n" +
            "  2. Notakto             — both players place X; whoever completes 3-in-a-row loses\n" +
            "  3. Gomoku              — place X/O on a 15x15 board; first to get 5-in-a-row wins\n\n" +
            "In-game commands:\n" +
            "  move <row> <col> <value>  — place a piece\n" +
            "  undo                      — revert the last move\n" +
            "  redo                      — reapply an undone move\n" +
            "  save <filename>           — save the current game state\n" +
            "  load <filename>           — load a saved game\n" +
            "  help                      — show available commands\n" +
            "  exit                      — return to the main menu");
    }
}
