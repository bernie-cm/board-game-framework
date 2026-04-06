namespace BoardGameFramework.Core;

// Abstract base class for all player types in the framework.
// Holds the properties shared by every player and declares MakeMove so every subclass provides its own input logic.
public abstract class Player {
    public int PlayerNumber { get; }
    public string GamePiece { get; }

    protected Player(int playerNumber, string gamePiece) {
        PlayerNumber = playerNumber;
        GamePiece = gamePiece;
    }

    // Each player type, Human or Computer, implements its own logic for choosing a move.
    public abstract (int row, int col, string value) MakeMove(Board board, IDisplay display);
}
