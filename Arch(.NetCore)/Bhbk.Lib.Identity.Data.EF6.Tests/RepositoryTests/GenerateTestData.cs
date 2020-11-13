using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF6.Infrastructure;
using Bhbk.Lib.Identity.Data.EF6.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using System;
using System.Data;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EF6.Tests.RepositoryTests
{
    public class GenerateTestData
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GenerateTestData(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow ?? throw new ArgumentNullException();
            _mapper = mapper ?? throw new ArgumentNullException();
        }

        public void Create()
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            /*
             * create test issuers
             */
            var foundIssuer = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<uvw_Issuer>()
                .Where(x => x.Name == Constants.TestIssuer).ToLambda())
                .SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = _uow.Issuers.Create(
                    _mapper.Map<uvw_Issuer>(new IssuerV1()
                    {
                        Name = Constants.TestIssuer,
                        IssuerKey = Constants.TestIssuerKey,
                        IsEnabled = true,
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }

            /*
             * create test audiences
             */
            var foundAudience = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<uvw_Audience>()
                .Where(x => x.Name == Constants.TestAudience).ToLambda())
                .SingleOrDefault();

            if (foundAudience == null)
            {
                foundAudience = _uow.Audiences.Create(
                    _mapper.Map<uvw_Audience>(new AudienceV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = Constants.TestAudience,
                        IsLockedOut = false,
                        IsEnabled = true,
                        IsDeletable = true,
                    }));

                _uow.Commit();

                _uow.Activities.Create(
                    _mapper.Map<uvw_Activity>(new ActivityV1()
                    {
                        AudienceId = foundAudience.Id,
                        ActivityType = LoginType.CreateAudienceAccessTokenV2.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Refreshes.Create(
                    _mapper.Map<uvw_Refresh>(new RefreshV1()
                    {
                        IssuerId = foundIssuer.Id,
                        AudienceId = foundAudience.Id,
                        RefreshType = RefreshType.Client.ToString(),
                        RefreshValue = AlphaNumeric.CreateString(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                _uow.Commit();
            }

            /*
             * create test claims
             */
            var foundClaim = _uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<uvw_Claim>()
                .Where(x => x.Type == Constants.TestClaim).ToLambda())
                .SingleOrDefault();

            if (foundClaim == null)
            {
                foundClaim = _uow.Claims.Create(
                    _mapper.Map<uvw_Claim>(new ClaimV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Subject = Constants.TestClaimSubject,
                        Type = Constants.TestClaim,
                        Value = AlphaNumeric.CreateString(8),
                        ValueType = Constants.TestClaimValueType,
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }

            /*
             * create test logins
             */
            var foundLogin = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<uvw_Login>()
                .Where(x => x.Name == Constants.TestLogin).ToLambda())
                .SingleOrDefault();

            if (foundLogin == null)
            {
                foundLogin = _uow.Logins.Create(
                    _mapper.Map<uvw_Login>(new LoginV1()
                    {
                        Name = Constants.TestLogin,
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }

            /*
             * create test roles
             */
            var foundRole = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<uvw_Role>()
                .Where(x => x.Name == Constants.TestRole).ToLambda())
                .SingleOrDefault();

            if (foundRole == null)
            {
                foundRole = _uow.Roles.Create(
                    _mapper.Map<uvw_Role>(new RoleV1()
                    {
                        AudienceId = foundAudience.Id,
                        Name = Constants.TestRole,
                        IsEnabled = true,
                        IsDeletable = true,
                    }));

                _uow.Commit();
            }

            /*
             * create test users
             */
            var foundUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_User>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda())
                .SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = _uow.Users.Create(
                    _mapper.Map<uvw_User>(new UserV1()
                    {
                        UserName = Constants.TestUser,
                        Email = Constants.TestUser,
                        PhoneNumber = NumberAs.CreateString(9),
                        FirstName = "First-" + AlphaNumeric.CreateString(4),
                        LastName = "Last-" + AlphaNumeric.CreateString(4),
                        IsLockedOut = false,
                        IsHumanBeing = true,
                        IsDeletable = true,
                    }));

                _uow.Commit();

                _uow.Activities.Create(
                    _mapper.Map<uvw_Activity>(new ActivityV1()
                    {
                        AudienceId = foundAudience.Id,
                        UserId = foundUser.Id,
                        ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                        IsDeletable = true,
                    }));

                _uow.Refreshes.Create(
                    _mapper.Map<uvw_Refresh>(new RefreshV1()
                    {
                        IssuerId = foundIssuer.Id,
                        AudienceId = foundAudience.Id,
                        UserId = foundUser.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = AlphaNumeric.CreateString(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                _uow.States.Create(
                    _mapper.Map<uvw_State>(new StateV1()
                    {
                        IssuerId = foundIssuer.Id,
                        AudienceId = foundAudience.Id,
                        UserId = foundUser.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                _uow.States.Create(
                    _mapper.Map<uvw_State>(new StateV1()
                    {
                        IssuerId = foundIssuer.Id,
                        AudienceId = foundAudience.Id,
                        UserId = foundUser.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = StateType.User.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                _uow.Commit();
            }
        }

        public void Destroy()
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            /*
             * delete test users
             */
            var users = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_User>()
                .Where(x => x.UserName.Contains(Constants.TestUser)).ToLambda());

            if(users.Count() > 0)
            {
                _uow.Users.Delete(users);
                _uow.Commit();
            }

            /*
             * delete test roles
             */
            var roles = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<uvw_Role>()
                .Where(x => x.Name.Contains(Constants.TestRole)).ToLambda());

            if (roles.Count() > 0)
            {
                _uow.Roles.Delete(roles);
                _uow.Commit();
            }

            /*
             * delete test logins
             */
            var logins = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<uvw_Login>()
                .Where(x => x.Name.Contains(Constants.TestLogin)).ToLambda());

            if (logins.Count() > 0)
            {
                _uow.Logins.Delete(logins);
                _uow.Commit();
            }

            /*
             * delete test claims
             */
            var claims = _uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<uvw_Claim>()
                .Where(x => x.Type.Contains(Constants.TestClaim)).ToLambda());

            if (claims.Count() > 0)
            {
                _uow.Claims.Delete(claims);
                _uow.Commit();
            }

            /*
             * delete test audiences
             */
            var audiences = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<uvw_Audience>()
                .Where(x => x.Name.Contains(Constants.TestAudience)).ToLambda());

            if (audiences.Count() > 0)
            {
                _uow.Audiences.Delete(audiences);
                _uow.Commit();
            }

            /*
             * delete test issuers
             */
            var issuers = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<uvw_Issuer>()
                .Where(x => x.Name.Contains(Constants.TestIssuer)).ToLambda());

            if (issuers.Count() > 0)
            {
                _uow.Issuers.Delete(issuers);
                _uow.Commit();
            }
        }
    }
}
