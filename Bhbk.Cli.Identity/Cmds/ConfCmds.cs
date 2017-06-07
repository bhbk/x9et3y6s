using Bhbk.Cli.Identity.Helper;
using Bhbk.Lib.Identity.Helper;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Manager;
using ManyConsole;
using System;

namespace Bhbk.Cli.Identity.Cmds
{
    public class ConfCmds : ConsoleCommand
    {
        private bool ReadConfig = false;

        public ConfCmds()
        {
            IsCommand("conf", "Do things with conf...");

            HasOption("r|read-config", "Read config data.", arg => { ReadConfig = true; });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                Statics.context = new CustomIdentityDbContext();
                Statics.uow = new UnitOfWork(Statics.context);
                
                if (ReadConfig)
                {
                    Console.WriteLine();
                    Console.WriteLine("\tPress key to read config data...");
                    Console.ReadKey();

                    Console.Write(Statics.uow.CustomConfigManager.ToString());

                    Console.WriteLine("\tCompleted read of config data...");
                    Console.WriteLine();
                }

                return MessageHelper.FondFarewell();
            }
            catch (Exception ex)
            {
                return MessageHelper.AngryFarewell(ex);
            }
        }
    }
}
