using System;

namespace Bhbk.Lib.Identity.Domain.Factories
{
    public interface IDefaultDataFactory
    {
        public void CreateAudiences();
        public void CreateAudienceRoles();
        public void CreateEntitlementTypes();
        public void CreateEntitlementScopes();
        public void CreateUserEntitlements();
        public void CreateAudienceEntitlements();
        public void CreateIssuers();
        public void CreateJobs();
        public void CreateLLMProviders();
        public void CreateLoginProviders();
        public void CreateRoles();
        public void CreateUsers();
        public void CreateUserLoginProviders();
        public void CreateUserRoles();
        public void Destroy();
    }
}
