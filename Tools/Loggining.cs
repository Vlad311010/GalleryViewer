namespace Tools
{
    internal static class Loggining
    {
        public static void Log(string str)
        {
            Console.WriteLine(str);
        }

        public static void Error(string str)
        {
            Console.WriteLine($"Error: {str}");
        }

        public static void Warning(string str)
        {
            Console.WriteLine($"Warning: {str}");
        }
    }
}
