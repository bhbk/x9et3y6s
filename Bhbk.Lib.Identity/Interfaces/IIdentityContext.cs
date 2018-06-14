using Bhbk.Lib.Identity.Managers;
using System;

namespace Bhbk.Lib.Identity.Interfaces
{
    //https://en.wikipedia.org/wiki/Dependency_inversion_principle
    public interface IIdentityContext : IDisposable
    {
        ContextType ContextStatus { get; }
        AudienceManager AudienceMgmt { get; }
        ClientManager ClientMgmt { get; }
        ConfigManager ConfigMgmt { get; }
        LoginManager LoginMgmt { get; }
        CustomRoleManager RoleMgmt { get; }
        CustomUserManager UserMgmt { get; }
    }

    public enum ContextType
    {
        UnitTest,
        Live
    }
}
