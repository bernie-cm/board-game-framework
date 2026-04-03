namespace BoardGameFramework;

/// <summary>
/// Concrete implementation of Numerical Tic-Tac-Toe.
///
/// Setup:
///   - 3x3 board, two players.
///   - Player 1 places ODD numbers  (1, 3, 5, 7, 9).
///   - Player 2 places EVEN numbers (2, 4, 6, 8).
///   - Numbers may never be reused.
///
/// Win condition:
///   - Any row, column or diagonal sums to exactly 15.
///
/// Draw condition:
///   - All 9 cells are filled with no line summing to 15.
///
/// Each turn the active player may:
///   - Place a number  →  enter row, col, value
///   - Undo last move  →  enter "undo"
///   - Redo last move  →  enter "redo"
///   - Save the game   →  enter "save"
/// </summary>
public class NumericalTicTacToeGame : Game
{
    private NumericalTicTacToeBoard _nttBoard;

    // Tracks who won so PrintWinner knows what to say.
    private Player? _winner;

    public NumericalTicTacToeGame(IDisplay display,
                                   HistoryManager historyManager,
                                   GameSaver gameSaver)
        : base(display, historyManager, gameSaver)
    {
        // board field is assigned in InitialiseGame; satisfy the compiler with a
        // temporary placeholder until then.
        _nttBoard = new NumericalTicTacToeBoard();
        board = _nttBoard;
    }

    // ── Template-method overrides ─────────────────────────────────────────────

    protected override void InitialiseGame()
    {
        _nttBoard = new NumericalTicTacToeBoard();
        board = _nttBoard;
        _winner = null;
        historyManager.Clear();

        // Player 1 → odd numbers, Player 2 → even numbers.
        // GamePiece is used here purely as a label shown in messages;
        // the actual pieces placed are the numbers chosen each turn.
        players.Clear();
        players.Add(new HumanPlayer(1, "Odd"));
        players.Add(new HumanPlayer(2, "Even"));
        currentPlayer = players[0];

        display.ShowMessage("=== Numerical Tic-Tac-Toe ===");
        display.ShowHelp(
            "Player 1 places ODD numbers (1,3,5,7,9). " +
            "Player 2 places EVEN numbers (2,4,6,8). " +
            "First to make a row/col/diagonal sum to 15 wins!\n" +
            "Commands: enter row col value (rows/cols are 1-3)  |  undo  |  redo  |  save <path>");
    }

    protected override void MakePlay(int playerIndex)
    {
        currentPlayer = players[playerIndex];
        bool isOddPlayer = currentPlayer.PlayerNumber == 1;

        // Show the current board state and whose turn it is.
        display.ShowBoard(board);
        var available = _nttBoard.GetAvailableNumbers(isOddPlayer);
        display.ShowMessage(
            $"\nPlayer {currentPlayer.PlayerNumber} ({currentPlayer.GamePiece}) — " +
            $"available numbers: [{string.Join(", ", available)}]");

        // Handle meta-commands first, then delegate move input to the player.
        while (true)
        {
            string metaInput = display.GetInput("Command (undo/redo/save <path>) or press enter to move: ").Trim();

            if (metaInput.Equals("undo", StringComparison.OrdinalIgnoreCase))
            {
                UndoMove();
                // SwitchPlayer inside UndoMove cancels the index bump PlayGame
                // would otherwise apply, so returning here replays this turn.
                return;
            }

            if (metaInput.Equals("redo", StringComparison.OrdinalIgnoreCase))
            {
                RedoMove();
                return;
            }

            if (metaInput.StartsWith("save ", StringComparison.OrdinalIgnoreCase))
            {
                string path = metaInput.Substring(5).Trim();
                SaveGame(path);
                continue; // Stay on the same player's turn after saving
            }

            if (metaInput.Length > 0)
            {
                display.ShowMessage("Unknown command. Use undo, redo, save <path>, or press enter to make a move.");
                continue;
            }

            // ── Delegate move input to the player ────────────────────────────
            // MakeMove handles prompting, parsing, 1-based conversion, bounds
            // checking and confirming the cell is empty. It loops internally
            // until those conditions are met, then returns (row, col, value).
            var (row, col, value) = currentPlayer.MakeMove(board, display);

            // ── NTT-specific validation on top of the generic move ────────────
            // HumanPlayer only knows about Board, so odd/even and number-range
            // rules are enforced here where we have access to NumericalTicTacToeBoard.
            if (!int.TryParse(value, out int number))
            {
                display.ShowMessage("Value must be a whole number between 1 and 9.");
                continue;
            }

            try
            {
                if (!_nttBoard.IsValidNTTMove(row, col, number, isOddPlayer))
                {
                    if (number < 1 || number > 9)
                        display.ShowMessage("Number must be between 1 and 9.");
                    else if (number % 2 == 0 && isOddPlayer)
                        display.ShowMessage("Player 1 must place ODD numbers.");
                    else if (number % 2 != 0 && !isOddPlayer)
                        display.ShowMessage("Player 2 must place EVEN numbers.");
                    else
                        display.ShowMessage("That number has already been placed. Choose a different number.");
                    continue;
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                display.ShowMessage($"Invalid position: {ex.Message}");
                continue;
            }

            // Valid move — execute through HistoryManager so undo/redo works.
            var command = new MoveCommand(_nttBoard, row, col, number.ToString());
            historyManager.Execute(command);
            return;
        }
    }

    protected override bool EndOfGame()
    {
        if (_nttBoard.CheckWin())
        {
            // The player who just moved is still currentPlayer at this point.
            _winner = currentPlayer;
            return true;
        }

        if (_nttBoard.IsFull())
        {
            _winner = null; // Draw
            return true;
        }

        return false;
    }

    protected override void PrintWinner()
    {
        display.ShowBoard(board);
        if (_winner != null)
            display.ShowResult(
                $"Player {_winner.PlayerNumber} ({_winner.GamePiece}) wins!");
        else
            display.ShowResult("It's a draw! No line sums to 15.");
    }

    // ── SaveData support ──────────────────────────────────────────────────────

    /// <summary>
    /// Restores board state and player list from a SaveData snapshot.
    /// Called by SaveData.RestoreGame after the game object is constructed.
    /// </summary>
    public void RestoreFromSaveData(SaveData data)
    {
        _nttBoard = new NumericalTicTacToeBoard();
        board = _nttBoard;
        _winner = null;

        // Restore grid
        if (data.Grid != null)
        {
            for (int r = 0; r < data.Grid.Count; r++)
                for (int c = 0; c < data.Grid[r].Count; c++)
                {
                    string? cell = data.Grid[r][c];
                    if (cell != null)
                        _nttBoard.PlaceMove(r, c, cell);
                }
        }

        // Restore players
        players.Clear();
        foreach (var pd in data.Players)
        {
            Player p = pd.IsHuman
                ? new HumanPlayer(pd.PlayerNumber, pd.GamePiece)
                : (Player)new HumanPlayer(pd.PlayerNumber, pd.GamePiece); // extend for ComputerPlayer
            players.Add(p);
        }

        currentPlayer = players.Count > data.CurrentPlayerIndex
            ? players[data.CurrentPlayerIndex]
            : players[0];
    }

    public override SaveData ToSaveData()
    {
        // Snapshot the grid as a List<List<string?>> — System.Text.Json does
        // not support 2D arrays, so we convert row by row.
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
            CurrentPlayerIndex = players.IndexOf(currentPlayer),
            Players = players.Select(p => new PlayerData
            {
                PlayerNumber = p.PlayerNumber,
                GamePiece = p.GamePiece,
                IsHuman = p is HumanPlayer
            }).ToList()
        };
    }
}
