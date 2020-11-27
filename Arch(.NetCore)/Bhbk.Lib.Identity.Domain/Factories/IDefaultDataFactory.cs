using System;

namespace Bhbk.Lib.Identity.Domain.Factories
{
    public interface IDefaultDataFactory
    {
        public void CreateAudiences();
        public void CreateAudienceRoles();
        public void CreateIssuers();
        public void CreateLogins();
        public void CreateRoles();
        public void CreateUsers();
        public void CreateUserLogins();
        public void CreateUserRoles();
        public void Destroy();
    }
}
