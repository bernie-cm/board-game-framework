namespace BoardGameFramework;

public class GameController
{
    private readonly GameFactory _gameFactory;
    private readonly IDisplay _display;

    public GameController(IDisplay display, GameFactory gameFactory)
    {
        _display = display;
        _gameFactory = gameFactory;
    }

    public void Start()
    {
        bool running = true;
        while (running)
        {
            running = ShowMainMenu();
        }
    }

    private bool ShowMainMenu()
    {
        _display.ShowMessage("\n=== Board Game Framework ===");
        _display.ShowMessage("1. New Game");
        _display.ShowMessage("2. Load Saved Game");
        _display.ShowMessage("3. Exit");

        string choice = _display.GetInput("Choose an option: ");
        switch (choice.Trim())
        {
            case "1":
                StartNewGame();
                return true;
            case "2":
                LoadSavedGame();
                return true;
            case "3":
                _display.ShowMessage("Goodbye!");
                return false;
            default:
                _display.ShowMessage("Invalid option. Please try again.");
                return true;
        }
    }

    private void StartNewGame()
    {
        _display.ShowMessage("\nSelect a game type:");
        _display.ShowMessage("1. Numerical Tic-Tac-Toe");
        string type = _display.GetInput("Game type: ");

        try
        {
            var game = _gameFactory.CreateGame(type, _display);
            game.PlayGame();
        }
        catch (ArgumentException ex)
        {
            _display.ShowMessage($"Error: {ex.Message}");
        }
    }

    private void LoadSavedGame()
    {
        string path = _display.GetInput("Enter save file path: ");
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
}
