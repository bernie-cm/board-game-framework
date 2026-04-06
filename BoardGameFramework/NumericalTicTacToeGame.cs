using BoardGameFramework.Core;
using BoardGameFramework.Commands;

namespace BoardGameFramework.Games;

// Concrete game class for Numerical Tic-Tac-Toe.
// Player 1 places odd numbers (1,3,5,7,9) and Player 2 places even numbers (2,4,6,8).
// The first player to make any row, column, or diagonal sum to 15 wins.
public class NumericalTicTacToeGame : Game
{
    private NumericalTicTacToeBoard _nttBoard;

    // Tracks who won so PrintWinner knows what to say. Null means a draw.
    private Player? _winner;

    // Creates a temporary board here to satisfy the compiler — InitialiseGame replaces it before play begins
    public NumericalTicTacToeGame(IDisplay display,
                                   HistoryManager historyManager,
                                   GameSaver gameSaver)
        : base(display, historyManager, gameSaver)
    {
        _nttBoard = new NumericalTicTacToeBoard();
        _board = _nttBoard;
    }

    // Allows GameFactory to inject the correct player types (human or computer) before the game starts
    public void SetPlayers(Player p1, Player p2)
    {
        _players.Clear();
        _players.Add(p1);
        _players.Add(p2);
    }

    // Resets the board and history, then sets the starting player.
    // Prompts for board size so the game can be played on any n×n grid (n ≥ 3).
    // Only creates default human players if none were injected via SetPlayers,
    // so that game mode selection from the menu is preserved.
    protected override void InitialiseGame()
    {
        int size = PromptBoardSize();
        _nttBoard = new NumericalTicTacToeBoard(size);
        _board = _nttBoard;
        _winner = null;
        _historyManager.Clear();

        if (_players.Count == 0)
        {
            _players.Add(new HumanPlayer(1, "Odd"));
            _players.Add(new HumanPlayer(2, "Even"));
        }
        _currentPlayer = _players[0];

        int maxNumber = size * size;
        _display.ShowMessage($"=== Numerical Tic-Tac-Toe ({size}×{size}) ===");
        _display.ShowHelp(
            $"Player 1 places ODD numbers. Player 2 places EVEN numbers. Numbers range from 1 to {maxNumber}.\n" +
            $"First to make any row, column, or diagonal sum to {_nttBoard.TargetSum} wins!\n" +
            "Commands: move <row> <col> <value>  |  undo  |  redo  |  save <file>  |  load <file>  |  help  |  exit");
    }

    // Asks the player for a board size and keeps asking until they enter a valid integer ≥ 3.
    // Pressing Enter without typing a number defaults to 3×3.
    private int PromptBoardSize()
    {
        while (true)
        {
            string input = _display.GetInput("Enter board size (3 or more, press Enter for default 3×3): ").Trim();
            if (string.IsNullOrEmpty(input)) return 3;
            if (int.TryParse(input, out int size) && size >= 3) return size;
            _display.ShowMessage("Board size must be a whole number of 3 or larger.");
        }
    }

    // Runs one player's turn. Computer players move immediately; human players enter commands
    // in a loop until they either make a valid move or trigger a meta-command (undo/redo/save/load/exit).
    protected override void MakePlay(int playerIndex)
    {
        _currentPlayer = _players[playerIndex];
        bool isOddPlayer = _currentPlayer.PlayerNumber == 1;

        _display.ShowBoard(_board);
        var available = _nttBoard.GetAvailableNumbers(isOddPlayer);
        _display.ShowMessage(
            $"\nPlayer {_currentPlayer.PlayerNumber} ({_currentPlayer.GamePiece}) — " +
            $"available numbers: [{string.Join(", ", available)}]");

        // Computer players bypass the command loop and move immediately
        if (_currentPlayer is ComputerPlayer)
        {
            var (row, col, value) = _currentPlayer.MakeMove(_board, _display);
            var cmd = new MoveCommand(_nttBoard, row, col, value);
            _historyManager.Execute(cmd);
            return;
        }

        while (true)
        {
            string input = _display.GetInput("> ").Trim();

            if (HandleMetaCommand(input)) return;

            if (input.StartsWith("move ", StringComparison.OrdinalIgnoreCase))
            {
                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 4 ||
                    !int.TryParse(parts[1], out int row) ||
                    !int.TryParse(parts[2], out int col) ||
                    !int.TryParse(parts[3], out int number))
                {
                    _display.ShowMessage("Usage: move <row> <col> <value>  (rows/cols are 1-3)");
                    continue;
                }
                // Player enters 1-based rows/cols; convert to 0-based for the board
                row--; col--;

                try
                {
                    if (!_nttBoard.IsValidNTTMove(row, col, number, isOddPlayer))
                    {
                        if (number < 1 || number > _nttBoard.Rows * _nttBoard.Cols)
                            _display.ShowMessage($"Number must be between 1 and {_nttBoard.Rows * _nttBoard.Cols}.");
                        else if (number % 2 == 0 && isOddPlayer)
                            _display.ShowMessage("Player 1 must place ODD numbers.");
                        else if (number % 2 != 0 && !isOddPlayer)
                            _display.ShowMessage("Player 2 must place EVEN numbers.");
                        else
                            _display.ShowMessage("That number has already been placed. Choose a different number.");
                        continue;
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    _display.ShowMessage($"Invalid position: {ex.Message}");
                    continue;
                }

                var command = new MoveCommand(_nttBoard, row, col, number.ToString());
                _historyManager.Execute(command);
                return;
            }

            _display.ShowMessage("Unknown command. Type 'help' for available commands.");
        }
    }

    // Handles commands that aren't moves. Returns true if the turn should end after this command,
    // false if the player should be prompted again (e.g. after saving or showing help).
    private bool HandleMetaCommand(string input)
    {
        if (input.Equals("undo", StringComparison.OrdinalIgnoreCase))
        {
            UndoMove();
            return true;
        }
        if (input.Equals("redo", StringComparison.OrdinalIgnoreCase))
        {
            RedoMove();
            return true;
        }
        if (input.StartsWith("save ", StringComparison.OrdinalIgnoreCase))
        {
            SaveGame(input.Substring(5).Trim());
            return false; // saving doesn't end the turn
        }
        if (input.StartsWith("load ", StringComparison.OrdinalIgnoreCase))
        {
            RequestLoad(input.Substring(5).Trim());
            return true;
        }
        if (input.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            _display.ShowHelp("move <row> <col> <value>  |  undo  |  redo  |  save <file>  |  load <file>  |  exit");
            return false;
        }
        if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            RequestExit();
            return true;
        }
        return false;
    }

    // Checks for a win after every move, then checks for a draw if the board is full
    protected override bool EndOfGame()
    {
        if (_nttBoard.CheckWin())
        {
            _winner = _currentPlayer;
            return true;
        }

        if (_nttBoard.IsFull())
        {
            _winner = null; // Draw
            return true;
        }

        return false;
    }

    // Shows the final board then announces the winner, or a draw if no line summed to 15
    protected override void PrintWinner()
    {
        _display.ShowBoard(_board);
        if (_winner != null)
            _display.ShowResult($"Player {_winner.PlayerNumber} ({_winner.GamePiece}) wins!");
        else
            _display.ShowResult("It's a draw! No line sums to 15.");
    }

    // Rebuilds the board and player list from saved data, then restores the undo/redo stacks.
    // Uses data.Rows to reconstruct the correct board size — a saved 4×4 game must reload as 4×4.
    // PlaceNTTMove is used instead of PlaceMove so _usedNumbers is repopulated correctly.
    public override void RestoreFromSaveData(SaveData data)
    {
        _nttBoard = new NumericalTicTacToeBoard(data.Rows);
        _board = _nttBoard;
        _winner = null;

        if (data.Grid != null)
        {
            for (int r = 0; r < data.Grid.Count; r++)
                for (int c = 0; c < data.Grid[r].Count; c++)
                {
                    string? cell = data.Grid[r][c];
                    if (cell != null && int.TryParse(cell, out int number))
                        _nttBoard.PlaceNTTMove(r, c, number);
                }
        }

        // Recreate computer players with the correct strategy rather than just marking them as human
        _players.Clear();
        foreach (var pd in data.Players)
        {
            Player p = pd.IsHuman
                ? new HumanPlayer(pd.PlayerNumber, pd.GamePiece)
                : new ComputerPlayer(pd.PlayerNumber, pd.GamePiece, new NTTComputerStrategy());
            _players.Add(p);
        }

        _currentPlayer = _players.Count > data.CurrentPlayerIndex ? _players[data.CurrentPlayerIndex] : _players[0];

        // Restore undo/redo stacks so the player can continue undoing and redoing after loading
        _historyManager.RestoreStacks(data.UndoStack, data.RedoStack, _nttBoard);
    }

    // Snapshots the full game state into a SaveData object, including the undo/redo stacks
    // so the player can continue undoing and redoing moves after loading the save file
    public override SaveData ToSaveData()
    {
        var gridCopy = new List<List<string?>>();
        for (int r = 0; r < _nttBoard.Rows; r++)
        {
            var row = new List<string?>();
            for (int c = 0; c < _nttBoard.Cols; c++)
                row.Add(_nttBoard.GetCell(r, c));
            gridCopy.Add(row);
        }

        return new SaveData
        {
            GameType = "ntt",
            Rows = _nttBoard.Rows,
            Cols = _nttBoard.Cols,
            Grid = gridCopy,
            CurrentPlayerIndex = _players.IndexOf(_currentPlayer),
            Players = _players.Select(p => new PlayerData
            {
                PlayerNumber = p.PlayerNumber,
                GamePiece = p.GamePiece,
                IsHuman = p is HumanPlayer
            }).ToList(),
            UndoStack = _historyManager.GetUndoStack(),
            RedoStack = _historyManager.GetRedoStack()
        };
    }
}

// AI strategy for Numerical Tic-Tac-Toe, implementing IComputerStrategy.
// Priority order: win immediately > block the opponent from winning > random valid move.
public class NTTComputerStrategy : IComputerStrategy
{
    private static readonly Random _rng = new Random();

    // Decides the best move by simulating each candidate on the board and checking the result.
    // The piece string ("Odd" or "Even") is used to determine which number pool the computer draws from.
    public (int row, int col, string value) ChooseMove(Board board, string piece)
    {
        var nttBoard = (NumericalTicTacToeBoard)board;
        bool isOdd = piece == "Odd";
        var myNumbers = nttBoard.GetAvailableNumbers(isOdd).ToList();
        var oppNumbers = nttBoard.GetAvailableNumbers(!isOdd).ToList();

        // Try every available number and cell — return immediately if a winning move is found
        foreach (int n in myNumbers)
            for (int r = 0; r < board.Rows; r++)
                for (int c = 0; c < board.Cols; c++)
                    if (nttBoard.IsValidNTTMove(r, c, n, isOdd))
                    {
                        nttBoard.PlaceMove(r, c, n.ToString());
                        bool wins = nttBoard.CheckWin();
                        nttBoard.RevertMove(r, c);
                        if (wins) return (r, c, n.ToString());
                    }

        // Simulate each opponent number to detect a threat, then block that cell with our own number
        foreach (int n in oppNumbers)
            for (int r = 0; r < board.Rows; r++)
                for (int c = 0; c < board.Cols; c++)
                    if (nttBoard.IsValidNTTMove(r, c, n, !isOdd))
                    {
                        nttBoard.PlaceMove(r, c, n.ToString());
                        bool oppWins = nttBoard.CheckWin();
                        nttBoard.RevertMove(r, c);
                        if (oppWins)
                        {
                            // Block by occupying that cell with one of our own numbers
                            var block = myNumbers.FirstOrDefault(mn => nttBoard.IsValidNTTMove(r, c, mn, isOdd));
                            if (block != 0) return (r, c, block.ToString());
                        }
                    }

        // No winning or blocking move found — pick a random valid move from what's available
        var moves = new List<(int r, int c, int n)>();
        foreach (int n in myNumbers)
            for (int r = 0; r < board.Rows; r++)
                for (int c = 0; c < board.Cols; c++)
                    if (nttBoard.IsValidNTTMove(r, c, n, isOdd))
                        moves.Add((r, c, n));

        var pick = moves[_rng.Next(moves.Count)];
        return (pick.r, pick.c, pick.n.ToString());
    }
}
