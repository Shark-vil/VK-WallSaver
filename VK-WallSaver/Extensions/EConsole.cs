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
                Answer = Console.ReadLine();
                Answer = Answer.Trim();
                Answer = Answer.ToLower();
            }
            while (Answer != "y" && Answer != "n");

            return Answer == "y";
        }

        public static bool HasNo() => !HasYes();

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
