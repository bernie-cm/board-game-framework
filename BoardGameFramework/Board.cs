namespace BoardGameFramework;

public abstract class Board {
  // Abstract class from whihc NTT, Gomoku and Notakto boards will inherit
  // This array will represent the board. Using string will accommodate different pieces of games, like "X", "O", or numbers
  protected string?[,] grid; 
  public int Rows { get; }
  public int Cols { get; }

  // Since Board is abstract, no-one should be able to instantiate a Board directly so the constructor is proetected
  protected Board(int rows, int cols) {
    Rows = rows;
    Cols = cols;
    grid = new string?[rows, cols];
  }

  public bool IsValidMove(int row, int col) {
    // This method checks if a piece in the game can be placed in a certain cell
    // It checks two things that must be true: position within bounds, and the cell is null (i.e., empty)
    if (row >= 0 && row < Rows && col >= 0 && col < Cols && grid[row, col] == null)
    {
      return true;
    } else
    {
      return false;
    }
  }

  public void PlaceMove(int row, int col, string value) {
  }

  public void RevertMove(int row, int col) {

  }

  public bool IsCellEmpty(int row, int col) {

  }

  public string? GetCell(int row, int col) {

  }

  // Abstract method that needs to be implemented by individual subclasses
  public abstract bool CheckWin();


}
