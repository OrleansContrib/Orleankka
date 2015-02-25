using System;

namespace Example
{
    public class ConsolePosition
    {
        static readonly object locker = new object();

        readonly int left;
        readonly int top;

        public ConsolePosition(int left, int top)
        {
            this.left = left;
            this.top = top;
        }

        public void Write(object obj)
        {
            Write(Console.ForegroundColor, obj);
        }

        public void Write(ConsoleColor color, object obj)
        {
            lock (locker)
            {
                var prevPosition = Current();
                var prevColor = Console.ForegroundColor;

                Console.ForegroundColor = color;
                Console.SetCursorPosition(left, top);
                Console.Write(obj);

                Console.SetCursorPosition(prevPosition.left, prevPosition.top);
                Console.ForegroundColor = prevColor;
            }
        }

        public ConsolePosition MoveLeft(int d)
        {
            return new ConsolePosition(left + d, top);
        }
        
        public ConsolePosition MoveTop(int d)
        {
            return new ConsolePosition(left, top + d);
        }

        public static ConsolePosition Current()
        {
            return new ConsolePosition(Console.CursorLeft, Console.CursorTop);
        }
    }
}