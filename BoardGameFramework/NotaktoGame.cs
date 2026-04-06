using BoardGameFramework.Core;
using BoardGameFramework.Commands;

namespace BoardGameFramework.Games;

// Notakto: both players place X on three independent 3×3 boards.
// A board is "dead" once it contains a complete line of three X's.
// The player who places the X that kills the last surviving board loses.
public class NotaktoGame : Game
{
    private NotaktoBoard _notaktoBoard;

    // Tracks who made the losing move (placed on the last live board to complete a line).
    // Null means the boards all filled without a line — an extremely unlikely draw.
    private Player? _loser;

    public NotaktoGame(IDisplay display, HistoryManager historyManager, GameSaver gameSaver)
        : base(display, historyManager, gameSaver)
    {
        _notaktoBoard = new NotaktoBoard();
        _board = _notaktoBoard;
    }

    // Allows GameFactory to inject the correct player types (human or computer) before the game starts
    public void SetPlayers(Player p1, Player p2)
    {
        _players.Clear();
        _players.Add(p1);
        _players.Add(p2);
    }

    // Resets all three boards and history then shows the rules.
    // Only creates default human players if none were injected via SetPlayers.
    protected override void InitialiseGame()
    {
        _notaktoBoard = new NotaktoBoard();
        _board = _notaktoBoard;
        _loser = null;
        _historyManager.Clear();

        if (_players.Count == 0)
        {
            _players.Add(new HumanPlayer(1, "X"));
            _players.Add(new HumanPlayer(2, "X"));
        }
        _currentPlayer = _players[0];

        _display.ShowMessage("=== Notakto (3 boards) ===");
        _display.ShowHelp(
            "Both players place X on any of the three boards. A board dies when it contains 3 X's in a row.\n" +
            "The player who kills the last surviving board LOSES!\n" +
            "Commands: move <board> <row> <col>  |  undo  |  redo  |  save <file>  |  load <file>  |  help  |  exit\n" +
            "  board, row, and col are all 1-based (1–3)");
    }

    // Runs one player's turn. Computer players move immediately; human players enter commands
    // until they make a valid move or trigger a meta-command.
    protected override void MakePlay(int playerIndex)
    {
        _currentPlayer = _players[playerIndex];

        _display.ShowMessage(_notaktoBoard.GetDisplayString());
        _display.ShowMessage($"Player {_currentPlayer.PlayerNumber}'s turn (place X).");

        if (_currentPlayer is ComputerPlayer)
        {
            // MakeMove calls ChooseMove which returns a global row already encoded by the strategy
            var (row, col, value) = _currentPlayer.MakeMove(_notaktoBoard, _display);
            _historyManager.Execute(new MoveCommand(_notaktoBoard, row, col, value));
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
                    !int.TryParse(parts[1], out int board) ||
                    !int.TryParse(parts[2], out int row) ||
                    !int.TryParse(parts[3], out int col))
                {
                    _display.ShowMessage("Usage: move <board> <row> <col>  (board/row/col are 1–3)");
                    continue;
                }

                // Player enters 1-based values; convert to 0-based for internal use
                int boardIndex = board - 1;
                int localRow = row - 1;
                int localCol = col - 1;

                if (boardIndex < 0 || boardIndex >= _notaktoBoard.BoardCount ||
                    localRow < 0 || localRow >= 3 || localCol < 0 || localCol >= 3)
                {
                    _display.ShowMessage("Board, row, and col must each be between 1 and 3.");
                    continue;
                }

                if (!_notaktoBoard.IsValidNotaktoMove(boardIndex, localRow, localCol))
                {
                    if (_notaktoBoard.IsBoardDead(boardIndex))
                        _display.ShowMessage($"Board {board} is dead — choose a different board.");
                    else
                        _display.ShowMessage("That cell is already occupied.");
                    continue;
                }

                int globalRow = NotaktoBoard.GlobalRow(boardIndex, localRow);
                _historyManager.Execute(new MoveCommand(_notaktoBoard, globalRow, localCol, "X"));
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
            return false;
        }
        if (input.StartsWith("load ", StringComparison.OrdinalIgnoreCase))
        {
            RequestLoad(input.Substring(5).Trim());
            return true;
        }
        if (input.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            _display.ShowHelp("move <board> <row> <col>  |  undo  |  redo  |  save <file>  |  load <file>  |  exit");
            return false;
        }
        if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            RequestExit();
            return true;
        }
        return false;
    }

    // The current player loses if they just killed the last live board.
    // A full board without any live boards remaining is also treated as an end (extremely rare draw).
    protected override bool EndOfGame()
    {
        if (_notaktoBoard.CheckWin())
        {
            _loser = _currentPlayer;
            return true;
        }
        if (_notaktoBoard.IsFull()) return true;
        return false;
    }

    // Shows the final board state then announces who lost (and therefore who won), or a draw
    protected override void PrintWinner()
    {
        _display.ShowMessage(_notaktoBoard.GetDisplayString());
        if (_loser != null)
        {
            var winner = _players.First(p => p != _loser);
            _display.ShowResult($"Player {_loser.PlayerNumber} killed the last board — Player {winner.PlayerNumber} wins!");
        }
        else
        {
            _display.ShowResult("All boards filled with no winner — it's a draw!");
        }
    }

    // Rebuilds all three boards and the player list from saved data, then restores the undo/redo stacks.
    // PlaceMove on NotaktoBoard automatically re-evaluates dead board status as pieces are placed.
    public override void RestoreFromSaveData(SaveData data)
    {
        _notaktoBoard = new NotaktoBoard();
        _board = _notaktoBoard;
        _loser = null;

        if (data.Grid != null)
            for (int r = 0; r < data.Grid.Count; r++)
                for (int c = 0; c < data.Grid[r].Count; c++)
                {
                    string? cell = data.Grid[r][c];
                    if (cell != null) _notaktoBoard.PlaceMove(r, c, cell);
                }

        _players.Clear();
        foreach (var pd in data.Players)
        {
            Player p = pd.IsHuman
                ? new HumanPlayer(pd.PlayerNumber, pd.GamePiece)
                : new ComputerPlayer(pd.PlayerNumber, pd.GamePiece, new NotaktoComputerStrategy());
            _players.Add(p);
        }

        _currentPlayer = _players.Count > data.CurrentPlayerIndex ? _players[data.CurrentPlayerIndex] : _players[0];
        _historyManager.RestoreStacks(data.UndoStack, data.RedoStack, _notaktoBoard);
    }

    // Snapshots the full game state into a SaveData object, including all three boards and the undo/redo stacks.
    // The 9×3 grid (3 stacked boards) serialises as-is so restoration is straightforward.
    public override SaveData ToSaveData()
    {
        var gridCopy = new List<List<string?>>();
        for (int r = 0; r < _notaktoBoard.Rows; r++)
        {
            var row = new List<string?>();
            for (int c = 0; c < _notaktoBoard.Cols; c++)
                row.Add(_notaktoBoard.GetCell(r, c));
            gridCopy.Add(row);
        }

        return new SaveData
        {
            GameType = "notakto",
            Rows = _notaktoBoard.Rows,
            Cols = _notaktoBoard.Cols,
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

// AI strategy for 3-board Notakto, implementing IComputerStrategy.
// Categorises every valid move into three tiers and picks from the safest available tier:
//   safe    — does not kill the board it is placed on
//   neutral — kills a board but other live boards remain (so game continues)
//   losing  — kills the only remaining live board (instant loss)
public class NotaktoComputerStrategy : IComputerStrategy
{
    private static readonly Random _rng = new Random();

    public (int row, int col, string value) ChooseMove(Board board, string piece)
    {
        var notaktoBoard = (NotaktoBoard)board;

        var safe    = new List<(int globalRow, int col)>();
        var neutral = new List<(int globalRow, int col)>();
        var losing  = new List<(int globalRow, int col)>();

        // Count how many boards are still alive so we can distinguish neutral from losing moves
        int liveBoards = 0;
        for (int b = 0; b < notaktoBoard.BoardCount; b++)
            if (!notaktoBoard.IsBoardDead(b)) liveBoards++;

        for (int boardIndex = 0; boardIndex < notaktoBoard.BoardCount; boardIndex++)
        {
            if (notaktoBoard.IsBoardDead(boardIndex)) continue;

            for (int localRow = 0; localRow < 3; localRow++)
                for (int col = 0; col < 3; col++)
                {
                    if (!notaktoBoard.IsValidNotaktoMove(boardIndex, localRow, col)) continue;

                    int globalRow = NotaktoBoard.GlobalRow(boardIndex, localRow);
                    bool killsBoard = notaktoBoard.WouldKillBoard(boardIndex, localRow, col);

                    if (!killsBoard)
                        safe.Add((globalRow, col));
                    else if (liveBoards > 1)
                        neutral.Add((globalRow, col));
                    else
                        losing.Add((globalRow, col));
                }
        }

        // Prefer safe moves; fall back to neutral if none, then losing if all moves end the game
        var pool = safe.Count > 0 ? safe : (neutral.Count > 0 ? neutral : losing);
        var pick = pool[_rng.Next(pool.Count)];
        return (pick.globalRow, pick.col, "X");
    }
}
