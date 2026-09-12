using Spectre.Console;

namespace Tools.Display
{
    internal static class ConsoleDisplay
    {
        public static void Display(string message, Color? color = null)
        {
            color ??= Color.Default;
            AnsiConsole.WriteLine(message, new Style(foreground: color));
        }

        public static void DisplayError(string message)
        {
            Display($"Error: {message}", Color.Red);
        }
    }
}
