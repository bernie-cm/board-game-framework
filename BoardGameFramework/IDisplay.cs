namespace BoardGameFramework;

public interface IDisplay
{
    void ShowBoard(Board board);
    void ShowMessage(string message);
    void ShowResult(string result);
    void ShowHelp(string help);
    string GetInput(string prompt);
}
