using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF6.Infrastructure;
using Bhbk.Lib.Identity.Data.EF6.Models;
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
            var foundIssuer = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<uvw_Issuers>()
                .Where(x => x.Name == Constants.ApiTestIssuer).ToLambda())
                .SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = _uow.Issuers.Create(
                    new uvw_Issuers()
                    {
                        Name = Constants.ApiTestIssuer,
                        IssuerKey = Constants.ApiTestIssuerKey,
                        Enabled = true,
                        Immutable = false,
                    });
            }

            /*
             * create test audiences
             */
            var foundAudience = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<uvw_Audiences>()
                .Where(x => x.Name == Constants.ApiTestAudience).ToLambda())
                .SingleOrDefault();

            if (foundAudience == null)
            {
                foundAudience = _uow.Audiences.Create(
                    new uvw_Audiences()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = Constants.ApiTestAudience,
                        AudienceType = AudienceType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    });

                _uow.Activities.Create(
                    new uvw_Activities()
                    {
                        AudienceId = foundAudience.Id,
                        ActivityType = LoginType.CreateAudienceAccessTokenV2.ToString(),
                        Immutable = false,
                    });

                _uow.Refreshes.Create(
                    new uvw_Refreshes()
                    {
                        IssuerId = foundIssuer.Id,
                        AudienceId = foundAudience.Id,
                        RefreshType = RefreshType.Client.ToString(),
                        RefreshValue = Base64.CreateString(8),
                        IssuedUtc = DateTime.UtcNow,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    });
            }

            /*
             * create test claims
             */
            var foundClaim = _uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<uvw_Claims>()
                .Where(x => x.Type == Constants.ApiTestClaim).ToLambda())
                .SingleOrDefault();

            if (foundClaim == null)
            {
                foundClaim = _uow.Claims.Create(
                    new uvw_Claims()
                    {
                        IssuerId = foundIssuer.Id,
                        Subject = Constants.ApiTestClaimSubject,
                        Type = Constants.ApiTestClaim,
                        Value = AlphaNumeric.CreateString(8),
                        ValueType = Constants.ApiTestClaimValueType,
                        Immutable = false,
                    });
            }

            /*
             * create test logins
             */
            var foundLogin = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<uvw_Logins>()
                .Where(x => x.Name == Constants.ApiTestLogin).ToLambda())
                .SingleOrDefault();

            if (foundLogin == null)
            {
                foundLogin = _uow.Logins.Create(
                    new uvw_Logins()
                    {
                        Name = Constants.ApiTestLogin,
                        LoginKey = Constants.ApiTestLoginKey,
                        Enabled = false,
                        Immutable = false,
                    });
            }

            /*
             * create test roles
             */
            var foundRole = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<uvw_Roles>()
                .Where(x => x.Name == Constants.ApiTestRole).ToLambda())
                .SingleOrDefault();

            if (foundRole == null)
            {
                foundRole = _uow.Roles.Create(
                    new uvw_Roles()
                    {
                        AudienceId = foundAudience.Id,
                        Name = Constants.ApiTestRole,
                        Enabled = true,
                        Immutable = false,
                    });
            }

            /*
             * create test users
             */
            var foundUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_Users>()
                .Where(x => x.UserName == Constants.ApiTestUser).ToLambda())
                .SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = _uow.Users.Create(
                    new uvw_Users()
                    {
                        UserName = Constants.ApiTestUser,
                        EmailAddress = Constants.ApiTestUser,
                        PhoneNumber = Constants.ApiTestUserPhone,
                        FirstName = "First-" + Base64.CreateString(4),
                        LastName = "Last-" + Base64.CreateString(4),
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = false,
                    });

                _uow.Activities.Create(
                    new uvw_Activities()
                    {
                        UserId = foundUser.Id,
                        ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                        Immutable = false,
                    });

                _uow.Refreshes.Create(
                    new uvw_Refreshes()
                    {
                        IssuerId = foundIssuer.Id,
                        UserId = foundUser.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = Base64.CreateString(8),
                        IssuedUtc = DateTime.UtcNow,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    });

                _uow.States.Create(
                    new uvw_States()
                    {
                        IssuerId = foundIssuer.Id,
                        AudienceId = foundAudience.Id,
                        UserId = foundUser.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                        LastPolling = DateTime.UtcNow,
                    });

                _uow.States.Create(
                    new uvw_States()
                    {
                        IssuerId = foundIssuer.Id,
                        AudienceId = foundAudience.Id,
                        UserId = foundUser.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = StateType.User.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                        LastPolling = DateTime.UtcNow,
                    });
            }
        }

        public void Destroy()
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            /*
             * delete test users
             */
            var users = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_Users>()
                .Where(x => x.UserName.Contains(Constants.ApiTestUser)).ToLambda());

            if(users.Count() > 0)
                _uow.Users.Delete(users);

            /*
             * delete test roles
             */
            var roles = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<uvw_Roles>()
                .Where(x => x.Name.Contains(Constants.ApiTestRole)).ToLambda());

            if (roles.Count() > 0)
                _uow.Roles.Delete(roles);

            /*
             * delete test logins
             */
            var logins = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<uvw_Logins>()
                .Where(x => x.Name.Contains(Constants.ApiTestLogin)).ToLambda());

            if (logins.Count() > 0)
                _uow.Logins.Delete(logins);

            /*
             * delete test claims
             */
            var claims = _uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<uvw_Claims>()
                .Where(x => x.Type.Contains(Constants.ApiTestClaim)).ToLambda());

            if (claims.Count() > 0)
                _uow.Claims.Delete(claims);

            /*
             * delete test audiences
             */
            var audiences = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<uvw_Audiences>()
                .Where(x => x.Name.Contains(Constants.ApiTestAudience)).ToLambda());

            if (audiences.Count() > 0)
                _uow.Audiences.Delete(audiences);

            /*
             * delete test issuers
             */
            var issuers = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<uvw_Issuers>()
                .Where(x => x.Name.Contains(Constants.ApiTestIssuer)).ToLambda());

            if (issuers.Count() > 0)
                _uow.Issuers.Delete(issuers);
        }
    }
}
