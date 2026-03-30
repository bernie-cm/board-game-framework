namespace BoardGameFramework;

public abstract class Board {
  // Abstract class from whihc NTT, Gomoku and Notakto boards will inherit
  // This array will represent the board. Using string will accommodate different pieces of games, like "X", "O", or numbers
  protected string?[,] grid; 
  public int Rows { get; }
  public int Cols { get; }

  // Since Board is abstract, no-one should be able to instantiate a Board directly so it's proetected
  protected Board(int rows, int cols) {
    Rows = rows;
    Cols = cols;
    grid = new string?[rows, cols];
  }
}
