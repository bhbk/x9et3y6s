using Bhbk.Lib.Identity.Infrastructure;
using System;
using System.Configuration;

namespace Bhbk.Cli.Identity
{
    internal enum ExitCodes : int
    {
        Success = 0,
        Failure = 1,
        Exception = 2
    }

    internal class Statics
    {
        internal static CustomIdentityDbContext context;
        internal static UnitOfWork uow;

        internal static readonly bool ConfDebug = Boolean.Parse(ConfigurationManager.AppSettings["Debug"]);
    }
}
