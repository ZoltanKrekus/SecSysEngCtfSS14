using System;
using System.Globalization;

namespace SpaceshipAI.UI.Backend
{
    public abstract class BackendScreen
    {
        public static void Write(object value)
        {
            Console.Write(value);
        }

        public static void WriteLine(object value)
        {
            Console.WriteLine(value);
        }

        public static void WriteColor(object value, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(value);
            Console.ResetColor();
        }

        public static void WriteLineColor(object value, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        public static void WriteError(string errorMessage)
        {
            WriteLineColor(errorMessage, ConsoleColor.Red);
        }

        public static void WriteHint(string hintMessage)
        {
            //WriteLineColor(">> " + hintMessage, ConsoleColor.DarkGray);
        }

        public static void WriteCode(string code)
        {
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(code);
            Console.ResetColor();
        }


        public static string ReadLine()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            string input = Console.ReadLine();
            Console.ResetColor();
            return input;
        }

        public static string ReadKey()
        {
            var ch = Console.ReadKey(true).KeyChar;
            return ch.ToString(CultureInfo.InvariantCulture);
        }

        public static void ClrScr()
        {
            try
            {
                Console.Clear();
            }
            catch
            {
                //May throw an exception when redirected...
            }
        }

        public abstract void Start();
    }
}