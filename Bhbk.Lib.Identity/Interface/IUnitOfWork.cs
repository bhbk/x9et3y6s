using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Manager;
using System;

namespace Bhbk.Lib.Identity.Interface
{
    //https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application
    public interface IUnitOfWork : IDisposable
    {
        ContextType ContextStatus { get; }
        ModelFactory Models { get; }
        AudienceManager AudienceMgmt { get; }
        ClientManager ClientMgmt { get; }
        ConfigManager ConfigMgmt { get; }
        ProviderManager ProviderMgmt { get; }
        CustomRoleManager RoleMgmt { get; }
        CustomUserManager UserMgmt { get; }
        void Save();
    }
}
