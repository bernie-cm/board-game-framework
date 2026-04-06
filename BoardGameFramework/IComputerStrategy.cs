namespace BoardGameFramework.Core;
public interface IComputerStrategy
{
    (int row, int col, string value) ChooseMove(Board board, string piece);
}