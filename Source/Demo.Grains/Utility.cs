using System;
using System.Linq;

namespace Demo
{
    public static class Log
    {
        static readonly object locker = new object();

        public static void Message(ConsoleColor color, string text, params object[] args)
        {
            lock (locker)
            {
                var prev = Console.ForegroundColor;

                Console.ForegroundColor = color;
                Console.WriteLine(text, args);

                Console.ForegroundColor = prev;
            }
        }
    }
}
