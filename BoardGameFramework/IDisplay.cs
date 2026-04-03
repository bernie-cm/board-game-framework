namespace BoardGameFramework;
public interface IDisplay
{
    void ShowBoard(Board board);
    void ShowMessage(string message);
    void ShowResult(string result);
    void ShowHelp(string helpText);
    string GetInput(string prompt);
}