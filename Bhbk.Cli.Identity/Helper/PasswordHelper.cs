using System;
using System.Text;

namespace Bhbk.Cli.Identity.Helper
{
    internal class PasswordHelper
    {
        internal static string GetStdin()
        {
            StringBuilder input = new StringBuilder();

            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);

                if (cki.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }

                Console.Write('*');
                input.Append(cki.KeyChar);
            }

            return input.ToString();
        }
    }
}
