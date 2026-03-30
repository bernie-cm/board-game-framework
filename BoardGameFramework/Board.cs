public abstract class Board {
  // Abstract class from whihc NTT, Gomoku and Notakto boards will inherit
  // This array will represent the board. Using string will accommodate different pieces of games, like "X", "O", or numbers
  protected string?[,] grid; 
  public int Rows { get; set; };
  public int Cols { get; set; };

  public Board(int rows, int cols) {
  }
}
