using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_WallSaver.Extensions
{
    public class EConsole
    {
        public static bool HasYes()
        {
            string Answer;

            do
            {
                Console.WriteLine("Y/n");
                Answer = ReadLine();
                Answer = Answer.Trim();
                Answer = Answer.ToLower();
            }
            while (Answer != "y" && Answer != "n");

            return Answer == "y";
        }

        public static bool HasNo() => !HasYes();

        // Source: https://stackoverflow.com/a/3404522/13457296
        public static string ReadPassword()
        {
            var pass = string.Empty;
            ConsoleKey key;

            Console.Write("> ");

            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    Console.Write("\b \b");
                    pass = pass[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    pass += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);

            Console.WriteLine();

            return pass;
        }

        public static string ReadLine(ConsoleColor TextColor = ConsoleColor.White)
        {
            string ReadText = string.Empty;
            ConsoleColor PreviewColor = Console.ForegroundColor;
            Console.ForegroundColor = TextColor;

            Console.Write("> ");
            ReadText = Console.ReadLine();

            Console.ForegroundColor = PreviewColor;

            return ReadText;
        }

        public static string ReadKey()
        {
            string Key = Console.ReadLine();
            Key = Key.Trim();
            Key = Key.ToLower();
            return Key;
        }

        public static void ClickToContinue()
        {
            Console.WriteLine("Press any button to continue (Example: Enter)");
            Console.Read();
        }

        public static void Print(object Text, ConsoleColor TextColor = ConsoleColor.White, string Prefix = "> ")
        {
            ConsoleColor PreviewColor = Console.ForegroundColor;
            Console.ForegroundColor = TextColor;
            Console.WriteLine(Prefix + Convert.ToString(Text));
            Console.ForegroundColor = PreviewColor;
        }

        public static void Error(object Text) => Print(Text, ConsoleColor.Red, "* ");

        public static void Warning(object Text) => Print(Text, ConsoleColor.Yellow, "! ");
    }
}
