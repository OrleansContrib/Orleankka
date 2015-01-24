using System;
using System.Linq;

namespace Demo
{
    public static class Log
    {
        public static void Message(ConsoleColor color, string text, params object[] args)
        {
            var prev = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.WriteLine(text, args);

            Console.ForegroundColor = prev;
        }
    }
}
