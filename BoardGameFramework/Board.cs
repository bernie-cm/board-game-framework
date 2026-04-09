namespace BoardGameFramework.Core;

public abstract class Board {
  // Abstract class from whihc NTT, Gomoku and Notakto boards will inherit
  // This array will represent the board. Using string will accommodate different pieces of games, like "X", "O", or numbers
  protected string?[,] _grid; 
  public int Rows { get; }
  public int Cols { get; }
  // Since Board is abstract, no-one should be able to instantiate a Board directly so the constructor is proetected
  protected Board(int rows, int cols) {
    Rows = rows;
    Cols = cols;
    _grid = new string?[rows, cols];
  }
  public bool IsValidMove(int row, int col) {
    // This method checks if a piece in the game can be placed in a certain cell
    // It checks two things that must be true: position within bounds, and the cell is null (i.e., empty)
    if (row >= 0 && row < Rows && col >= 0 && col < Cols && _grid[row, col] == null) {
      return true;
    } else {
      return false;
    }
  }
  // Marked virtual so subclasses like NumericalTicTacToeBoard can override it to run additional logic whenever a piece is placed
  public virtual void PlaceMove(int row, int col, string value) {
    // This method simply validates first, before setting the grid location to the value
    if (IsValidMove(row, col)) {
      _grid[row, col] = value;
    }
  }
  // Same as PlaceMove
  public virtual void RevertMove(int row, int col) {
    // Method to undo PlaceMove action. This method is called by MoveCommand.Undo() to reverse a move
    _grid[row, col] = null;
  }
  // Method checks if the position is empty, and returns false if there's a value
  public bool IsCellEmpty(int row, int col) => _grid[row, col] == null;
  public string? GetCell(int row, int col) {
    // Method used to read cell values for rendering by ConsoleDisplay.ShowBoard()
    // First need to check if the position requested is within bounds
    if (row >= 0 && row < Rows && col >= 0 && col < Cols) {
      return _grid[row, col];
    } else {
      return null; // If out of bounds, return null
    }
  }
  // Abstract method that needs to be implemented by individual subclasses
  public abstract bool CheckWin();
}