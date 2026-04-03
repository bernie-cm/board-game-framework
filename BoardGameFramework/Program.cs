using BoardGameFramework;

var display = new ConsoleDisplay();
var factory = new GameFactory();
var controller = new GameController(display, factory);
controller.Start();
