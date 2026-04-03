namespace BoardGameFramework;
public interface IDisplay
{
    /*
    The IDisplay interface defines the contract that defines every way the 
    game framework talks to the user. For example, showing messages, showing the board
    and getting input. For this framework, the implementation is defined separately in the
    ConsoleDisplay.cs class
    */
    void ShowBoard(Board board);
    void ShowMessage(string message);
    void ShowResult(string result);
    void ShowHelp(string helpText);
    string GetInput(string prompt);
}