namespace Tools.Display
{
    internal static class ConsoleDisplay
    {
        public static void Display(string str)
        {
            Console.WriteLine(str);
        }

        public static void DisplayError(string str)
        {
            Console.WriteLine($"Error: {str}");
        }
    }
}
