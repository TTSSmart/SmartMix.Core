namespace SmartMix.Core.Common.Helpers
{
    public static class ExConsole
    {
        static ExConsole()
        {
            ErrorColor = ConsoleColor.Red;
        }

        /// <summary>
        /// Цвет сообщений, выводимых при ошибке.
        /// </summary>
        public static ConsoleColor ErrorColor { get; set; }

        public static void WriteLine(string text, ConsoleColor foregroundColor, params object[] args)
        {
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(text, args);
            Console.ForegroundColor = defaultColor;
        }

        public static void WriteLine(string text, ConsoleColor foregroundColor)
        {
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(text);
            Console.ForegroundColor = defaultColor;
        }

        public static void Write(string text, ConsoleColor foregroundColor, params object[] args)
        {
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.Write(text, args);
            Console.ForegroundColor = defaultColor;
        }

        public static void Write(string text, ConsoleColor foregroundColor)
        {
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.Write(text);
            Console.ForegroundColor = defaultColor;
        }

        public static void WriteError(string text)
        {
            Write(text, ErrorColor);
        }

        public static void WriteError(string text, params object[] args)
        {
            Write(text, ErrorColor, args);
        }

        public static void WriteErrorLine(string text)
        {
            WriteLine(text, ErrorColor);
        }

        public static void WriteErrorLine(string text, params object[] args)
        {
            WriteLine(text, ErrorColor, args);
        }

        public static void WriteSeparatorLine(char segment, ConsoleColor foregroundColor, string text = "")
        {
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;
            WriteSeparatorLine(segment, text);
            Console.ForegroundColor = defaultColor;
        }

        public static void WriteSeparatorLine(char segment, string text = "")
        {
            string result = (text.Length > 0) ? (text + " ") : "";
            int iterations = (text.Length > 0) ? (80 - text.Length - 1) : 80;
            for (int i = 0; i < iterations; i++)
            {
                result += segment;
            }
            Console.WriteLine(result);
        }
    }
}
