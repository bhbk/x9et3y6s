using Bhbk.Cli.Identity.Primitives;
using System;

namespace Bhbk.Cli.Identity.Helpers
{
    internal class MessageHelper
    {
        internal static int FondFarewell()
        {
            Console.WriteLine();
            Console.Write("Press key to exit...");
            Console.ReadKey();
            return (int)ExitCodes.Success;
        }

        internal static int AngryFarewell(Exception ex)
        {
            Console.WriteLine();
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.StackTrace);
            Console.WriteLine();
            Console.Write("Press key to exit...");

            Console.ReadKey();
            return (int)ExitCodes.Exception;
        }
    }
}
