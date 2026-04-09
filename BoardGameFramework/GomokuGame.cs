using BoardGameFramework.Core;
using BoardGameFramework.Commands;
namespace BoardGameFramework.Games;

// Gomoku: two players alternate placing stones on a 15x15 board.
// The first player to form an unbroken line of five stones in any direction wins.
public class GomokuGame : Game {
    private GomokuBoard _gomokuBoard;
    // Tracks who formed the winning five-in-a-row. Null means a draw.
    private Player? _winner;
    public GomokuGame(IDisplay display, HistoryManager historyManager, GameSaver gameSaver)
        : base(display, historyManager, gameSaver) {
        _gomokuBoard = new GomokuBoard();
        _board = _gomokuBoard;
    }
    // Allows GameFactory to inject the correct player types (human or computer) before the game starts
    public void SetPlayers(Player p1, Player p2) {
        _players.Clear();
        _players.Add(p1);
        _players.Add(p2);
    }
    // Resets the board and history then shows the rules.
    // Only creates default human players if none were injected via SetPlayers.
    protected override void InitialiseGame() {
        _gomokuBoard = new GomokuBoard();
        _board = _gomokuBoard;
        _winner = null;
        _historyManager.Clear();

        if (_players.Count == 0) {
            _players.Add(new HumanPlayer(1, "X"));
            _players.Add(new HumanPlayer(2, "O"));
        }
        _currentPlayer = _players[0];

        _display.ShowMessage("=== Gomoku ===");
        _display.ShowHelp(
            "Player 1 is X, Player 2 is O. Place 5 in a row to win! Board is 15x15.\n" +
            "Commands: move <row> <col>  |  undo  |  redo  |  save <file>  |  load <file>  |  help  |  exit");
    }
    // Runs one player's turn. Computer players move immediately; human players enter commands
    // until they make a valid move or trigger a meta-command.
    protected override void MakePlay(int playerIndex) {
        _currentPlayer = _players[playerIndex];
        _display.ShowBoard(_board);
        _display.ShowMessage($"\nPlayer {_currentPlayer.PlayerNumber} ({_currentPlayer.GamePiece})'s turn.");

        if (_currentPlayer is ComputerPlayer) {
            var (row, col, value) = _currentPlayer.MakeMove(_board, _display);
            _historyManager.Execute(new MoveCommand(_gomokuBoard, row, col, value));
            return;
        }

        while (true) {
            string input = _display.GetInput("> ").Trim();

            if (HandleMetaCommand(input)) return;

            if (input.StartsWith("move ", StringComparison.OrdinalIgnoreCase)) {
                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3 ||
                    !int.TryParse(parts[1], out int row) ||
                    !int.TryParse(parts[2], out int col))
                {
                    _display.ShowMessage("Usage: move <row> <col>  (rows/cols are 1-15)");
                    continue;
                }
                // Player enters 1-based rows/cols; convert to 0-based for the board
                row--; col--;

                if (!_board.IsValidMove(row, col)) {
                    _display.ShowMessage("Invalid position. The cell must be empty and within the board.");
                    continue;
                }

                _historyManager.Execute(new MoveCommand(_gomokuBoard, row, col, _currentPlayer.GamePiece));
                return;
            }
            _display.ShowMessage("Unknown command. Type 'help' for available commands.");
        }
    }
    // Handles commands that aren't moves. Returns true if the turn should end after this command,
    // false if the player should be prompted again (e.g. after saving or showing help).
    private bool HandleMetaCommand(string input) {
        if (input.Equals("undo", StringComparison.OrdinalIgnoreCase)) {
            UndoMove();
            return true;
        }
        if (input.Equals("redo", StringComparison.OrdinalIgnoreCase)) {
            RedoMove();
            return true;
        }
        if (input.StartsWith("save ", StringComparison.OrdinalIgnoreCase)) {
            SaveGame(input.Substring(5).Trim());
            return false;
        }
        if (input.StartsWith("load ", StringComparison.OrdinalIgnoreCase)) {
            RequestLoad(input.Substring(5).Trim());
            return true;
        }
        if (input.Equals("help", StringComparison.OrdinalIgnoreCase)) {
            _display.ShowHelp("move <row> <col>  |  undo  |  redo  |  save <file>  |  load <file>  |  exit");
            return false;
        }
        if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) {
            RequestExit();
            return true;
        }
        return false;
    }
    // The current player wins if they just completed five in a row. A full board is a draw.
    protected override bool EndOfGame() {
        if (_gomokuBoard.CheckWin()) {
            _winner = _currentPlayer;
            return true;
        }
        if (_gomokuBoard.IsFull()) return true;
        return false;
    }
    // Shows the final board then announces the winner or a draw
    protected override void PrintWinner() {
        _display.ShowBoard(_board);
        if (_winner != null)
            _display.ShowResult($"Player {_winner.PlayerNumber} ({_winner.GamePiece}) wins with five in a row!");
        else
            _display.ShowResult("The board is full — it's a draw!");
    }
    // Rebuilds the board and player list from saved data, then restores the undo/redo stacks
    public override void RestoreFromSaveData(SaveData data) {
        _gomokuBoard = new GomokuBoard();
        _board = _gomokuBoard;
        _winner = null;

        if (data.Grid != null)
            for (int r = 0; r < data.Grid.Count; r++)
                for (int c = 0; c < data.Grid[r].Count; c++) {
                    string? cell = data.Grid[r][c];
                    if (cell != null) _gomokuBoard.PlaceMove(r, c, cell);
                }

        _players.Clear();
        foreach (var pd in data.Players) {
            Player p = pd.IsHuman
                ? new HumanPlayer(pd.PlayerNumber, pd.GamePiece)
                : new ComputerPlayer(pd.PlayerNumber, pd.GamePiece, new GomokuComputerStrategy());
            _players.Add(p);
        }

        _currentPlayer = _players.Count > data.CurrentPlayerIndex ? _players[data.CurrentPlayerIndex] : _players[0];
        _historyManager.RestoreStacks(data.UndoStack, data.RedoStack, _gomokuBoard);
    }
    // Snapshots the full game state into a SaveData object, including the undo/redo stacks
    public override SaveData ToSaveData() {
        var gridCopy = new List<List<string?>>();
        for (int r = 0; r < _gomokuBoard.Rows; r++) {
            var row = new List<string?>();
            for (int c = 0; c < _gomokuBoard.Cols; c++)
                row.Add(_gomokuBoard.GetCell(r, c));
            gridCopy.Add(row);
        }

        return new SaveData
        {
            GameType = "gomoku",
            Rows = _gomokuBoard.Rows,
            Cols = _gomokuBoard.Cols,
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
// AI strategy for Gomoku, implementing IComputerStrategy.
// Priority order: win immediately > block opponent from winning > score cells by sequence length > random.
public class GomokuComputerStrategy : IComputerStrategy {
    private static readonly Random _rng = new Random();
    private static readonly (int dRow, int dCol)[] _directions =
        { (0, 1), (1, 0), (1, 1), (1, -1) };
    // Scores every empty cell and picks the best one.
    // Winning and blocking moves are assigned fixed high scores so they always take priority
    // over positional scoring.
    public (int row, int col, string value) ChooseMove(Board board, string piece) {
        var gomokuBoard = (GomokuBoard)board;
        string oppPiece = piece == "X" ? "O" : "X";

        int bestScore = -1;
        var bestMoves = new List<(int r, int c)>();

        for (int r = 0; r < board.Rows; r++)
            for (int c = 0; c < board.Cols; c++)
            {
                if (!board.IsValidMove(r, c)) continue;

                int score = ScoreCell(gomokuBoard, r, c, piece, oppPiece);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMoves.Clear();
                    bestMoves.Add((r, c));
                }
                else if (score == bestScore)
                {
                    bestMoves.Add((r, c));
                }
            }

        // If the board is completely empty, start in the centre
        if (bestMoves.Count == 0)
            return (board.Rows / 2, board.Cols / 2, piece);

        var pick = bestMoves[_rng.Next(bestMoves.Count)];
        return (pick.r, pick.c, piece);
    }
    // Assigns a score to placing at (row, col).
    // Checks for an immediate win or a block first, then falls back to a sequence-length heuristic.
    private static int ScoreCell(GomokuBoard board, int row, int col, string mine, string opp) {
        // Check if placing here wins the game
        board.PlaceMove(row, col, mine);
        if (board.CheckWin()) { board.RevertMove(row, col); return 100000; }
        board.RevertMove(row, col);

        // Check if the opponent would win here and must be blocked
        board.PlaceMove(row, col, opp);
        if (board.CheckWin()) { board.RevertMove(row, col); return 90000; }
        board.RevertMove(row, col);

        // Score based on how long a sequence this cell would join or create.
        // Opponent threats are weighted at half to ensure the AI prefers attacking over defending
        // when neither player is close to winning.
        int score = 0;
        foreach (var (dRow, dCol) in _directions)
        {
            int myLine = 1
                + board.CountInDirection(row, col, mine, dRow, dCol)
                + board.CountInDirection(row, col, mine, -dRow, -dCol);
            int oppLine = 1
                + board.CountInDirection(row, col, opp, dRow, dCol)
                + board.CountInDirection(row, col, opp, -dRow, -dCol);

            score += myLine * myLine;
            score += oppLine * oppLine / 2;
        }
        return score;
    }
}