using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data_EF6.Infrastructure_Tbl;
using Bhbk.Lib.Identity.Data_EF6.Models_Tbl;
using Bhbk.Lib.Identity.Domain.Profiles;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.Identity.Primitives.Tests.Constants;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using System;
using System.Data;
using System.Linq;

namespace Bhbk.Lib.Identity.Data_EF6.Tests.RepositoryTests_Tbl
{
    public class TestDataFactory : IDisposable
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _map;
        private tbl_Issuer foundIssuer;
        private tbl_Audience foundAudience;
        private tbl_Login foundLogin;
        private tbl_Claim foundClaim;
        private tbl_Role foundRole;
        private tbl_User foundUser;
        private bool disposedValue;

        public TestDataFactory(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException();

            if (_uow.InstanceType == InstanceContext.DeployedOrLocal
                || _uow.InstanceType == InstanceContext.End2EndTest)
                throw new InvalidOperationException();

            _map = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EF6_TBL>()).CreateMapper();
        }

        public void CreateAudiences()
        {
            if (foundIssuer == null)
                CreateIssuers();

            /*
             * create test audiences
             */

            foundAudience = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name == TestDefaultConstants.AudienceName).ToLambda())
                .SingleOrDefault();

            if (foundAudience == null)
            {
                foundAudience = _uow.Audiences.Create(
                    _map.Map<tbl_Audience>(new AudienceV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = TestDefaultConstants.AudienceName,
                        IsLockedOut = false,
                        IsDeletable = true,
                    }));

                _uow.Commit();

                _uow.AuthActivity.Create(
                    _map.Map<tbl_AuthActivity>(new AuthActivityV1()
                    {
                        AudienceId = foundAudience.Id,
                        LoginType = GrantFlowType.ClientCredentialV2.ToString(),
                        LoginOutcome = GrantFlowResultType.Success.ToString(),
                    }));

                _uow.Commit();
            }
        }

        public void CreateAudienceRefreshes()
        {
            if (foundIssuer == null)
                CreateIssuers();

            if (foundAudience == null)
                CreateAudiences();

            /*
             * create test refreshes
             */

            _uow.Refreshes.Create(
                _map.Map<tbl_Refresh>(new RefreshV1()
                {
                    IssuerId = foundIssuer.Id,
                    AudienceId = foundAudience.Id,
                    RefreshType = ConsumerType.Client.ToString(),
                    RefreshValue = AlphaNumeric.CreateString(8),
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                }));

            _uow.Commit();
        }

        public void CreateClaims()
        {
            if (foundIssuer == null)
                CreateIssuers();

            /*
             * create test claims
             */

            foundClaim = _uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.Type == TestDefaultConstants.ClaimName).ToLambda())
                .SingleOrDefault();

            if (foundClaim == null)
            {
                foundClaim = _uow.Claims.Create(
                    _map.Map<tbl_Claim>(new ClaimV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Subject = TestDefaultConstants.ClaimSubject,
                        Type = TestDefaultConstants.ClaimName,
                        Value = AlphaNumeric.CreateString(8),
                        ValueType = TestDefaultConstants.ClaimValueType,
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }
        }

        public void CreateIssuers()
        {
            /*
             * create test issuers
             */

            foundIssuer = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Name == TestDefaultConstants.IssuerName).ToLambda())
                .SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = _uow.Issuers.Create(
                    _map.Map<tbl_Issuer>(new IssuerV1()
                    {
                        Name = TestDefaultConstants.IssuerName,
                        IssuerKey = TestDefaultConstants.IssuerKey,
                        IsEnabled = true,
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }
        }

        public void CreateLogins()
        {
            /*
             * create test logins
             */

            foundLogin = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<tbl_Login>()
                .Where(x => x.Name == TestDefaultConstants.LoginName).ToLambda())
                .SingleOrDefault();

            if (foundLogin == null)
            {
                foundLogin = _uow.Logins.Create(
                    _map.Map<tbl_Login>(new LoginV1()
                    {
                        Name = TestDefaultConstants.LoginName,
                        LoginKey = AlphaNumeric.CreateString(16),
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }
        }

        public void CreateRoles()
        {
            if (foundAudience == null)
                CreateAudiences();

            /*
             * create test roles
             */

            foundRole = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.Name == TestDefaultConstants.RoleName).ToLambda())
                .SingleOrDefault();

            if (foundRole == null)
            {
                foundRole = _uow.Roles.Create(
                    _map.Map<tbl_Role>(new RoleV1()
                    {
                        AudienceId = foundAudience.Id,
                        Name = TestDefaultConstants.RoleName,
                        IsEnabled = true,
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }
        }

        public void CreateUsers()
        {
            /*
             * create test users
             */

            foundUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.UserName == TestDefaultConstants.UserName).ToLambda())
                .SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = _uow.Users.Create(
                    _map.Map<tbl_User>(new UserV1()
                    {
                        UserName = TestDefaultConstants.UserName,
                        Email = TestDefaultConstants.UserName,
                        PhoneNumber = NumberAs.CreateString(11),
                        FirstName = "First-" + AlphaNumeric.CreateString(4),
                        LastName = "Last-" + AlphaNumeric.CreateString(4),
                        IsLockedOut = false,
                        IsHumanBeing = true,
                        IsDeletable = true,
                    }));

                _uow.Commit();

                _uow.AuthActivity.Create(
                    _map.Map<tbl_AuthActivity>(new AuthActivityV1()
                    {
                        UserId = foundUser.Id,
                        LoginType = GrantFlowType.ResourceOwnerPasswordV2.ToString(),
                        LoginOutcome = GrantFlowResultType.Success.ToString(),
                    }));

                _uow.Commit();
            }
        }

        public void CreateUserRefreshes()
        {
            if (foundIssuer == null)
                CreateIssuers();

            if (foundAudience == null)
                CreateAudiences();

            if (foundUser == null)
                CreateUsers();

            /*
             * create test refreshes
             */

            _uow.Refreshes.Create(
                _map.Map<tbl_Refresh>(new RefreshV1()
                {
                    IssuerId = foundIssuer.Id,
                    AudienceId = foundAudience.Id,
                    UserId = foundUser.Id,
                    RefreshType = ConsumerType.User.ToString(),
                    RefreshValue = AlphaNumeric.CreateString(8),
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                }));

            _uow.Commit();
        }

        public void CreateUserStates()
        {
            if (foundIssuer == null)
                CreateIssuers();

            if (foundAudience == null)
                CreateAudiences();

            if (foundUser == null)
                CreateUsers();

            /*
             * create test states
             */

            _uow.States.Create(
                _map.Map<tbl_State>(new StateV1()
                {
                    IssuerId = foundIssuer.Id,
                    AudienceId = foundAudience.Id,
                    UserId = foundUser.Id,
                    StateValue = AlphaNumeric.CreateString(32),
                    StateType = ConsumerType.Device.ToString(),
                    StateConsume = true,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                }));

            _uow.States.Create(
                _map.Map<tbl_State>(new StateV1()
                {
                    IssuerId = foundIssuer.Id,
                    AudienceId = foundAudience.Id,
                    UserId = foundUser.Id,
                    StateValue = AlphaNumeric.CreateString(32),
                    StateType = ConsumerType.User.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                }));

            _uow.Commit();
        }

        public void Destroy()
        {
            /*
             * delete test users
             */

            var users = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.UserName.Contains(TestDefaultConstants.UserName)).ToLambda());

            if (users.Count() > 0)
            {
                _uow.Users.Delete(users);
                _uow.Commit();
            }

            /*
             * delete test roles
             */

            var roles = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                .Where(x => x.Name.Contains(TestDefaultConstants.RoleName)).ToLambda());

            if (roles.Count() > 0)
            {
                _uow.Roles.Delete(roles);
                _uow.Commit();
            }

            /*
             * delete test logins
             */

            var logins = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<tbl_Login>()
                .Where(x => x.Name.Contains(TestDefaultConstants.LoginName)).ToLambda());

            if (logins.Count() > 0)
            {
                _uow.Logins.Delete(logins);
                _uow.Commit();
            }

            /*
             * delete test claims
             */

            var claims = _uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                .Where(x => x.Type.Contains(TestDefaultConstants.ClaimName)).ToLambda());

            if (claims.Count() > 0)
            {
                _uow.Claims.Delete(claims);
                _uow.Commit();
            }

            /*
             * delete test audiences
             */

            var audiences = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name.Contains(TestDefaultConstants.AudienceName)).ToLambda());

            if (audiences.Count() > 0)
            {
                _uow.Audiences.Delete(audiences);
                _uow.Commit();
            }

            /*
             * delete test issuers
             */

            var issuers = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                .Where(x => x.Name.Contains(TestDefaultConstants.IssuerName)).ToLambda());

            if (issuers.Count() > 0)
            {
                _uow.Issuers.Delete(issuers);
                _uow.Commit();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Destroy();
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TestDataFactory_TBL()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
