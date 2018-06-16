using System;
using System.Text;

namespace Bhbk.Cli.Identity.Helpers
{
    internal class ConsoleHelper
    {
        internal static string GetInput()
        {
            StringBuilder sb = new StringBuilder();

            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }

                if (cki.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                    {
                        Console.Write("\b\0\b");
                        sb.Length--;
                    }

                    continue;
                }

                Console.Write(cki.KeyChar);
                sb.Append(cki.KeyChar);
            }

            return sb.ToString();
        }

        internal static string GetHiddenInput()
        {
            StringBuilder sb = new StringBuilder();

            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);

                if (cki.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }

                Console.Write('*');
                sb.Append(cki.KeyChar);
            }

            return sb.ToString();
        }
    }
}
