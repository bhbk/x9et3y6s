using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.EF6.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Data.EF6.Models_DIRECT;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using System;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EF6.Tests.RepositoryTests_DIRECT
{
    public class GenerateTestData
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ValidationHelper _validate;

        public GenerateTestData(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow ?? throw new ArgumentNullException();
            _mapper = mapper ?? throw new ArgumentNullException();
            _validate = new ValidationHelper();
        }

        public void Create()
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            /*
             * create test issuers
             */
            var foundIssuer = _uow.Issuers.Get(new QueryExpression<tbl_Issuers>()
                .Where(x => x.Name == Constants.ApiTestIssuer).ToLambda())
                .SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = _uow.Issuers.Create(
                    _mapper.Map<tbl_Issuers>(new IssuerV1()
                    {
                        Name = Constants.ApiTestIssuer,
                        IssuerKey = Constants.ApiTestIssuerKey,
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Commit();
            }

            /*
             * create test audiences
             */
            var foundAudience = _uow.Audiences.Get(new QueryExpression<tbl_Audiences>()
                .Where(x => x.Name == Constants.ApiTestAudience).ToLambda())
                .SingleOrDefault();

            if (foundAudience == null)
            {
                foundAudience = _uow.Audiences.Create(
                    _mapper.Map<tbl_Audiences>(new AudienceV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = Constants.ApiTestAudience,
                        AudienceType = AudienceType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Activities.Create(
                    _mapper.Map<tbl_Activities>(new ActivityV1()
                    {
                        AudienceId = foundAudience.Id,
                        ActivityType = LoginType.CreateAudienceAccessTokenV2.ToString(),
                        Immutable = false,
                    }));

                _uow.Refreshes.Create(
                    _mapper.Map<tbl_Refreshes>(new RefreshV1()
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
             * create test client urls
             */
            var url = new Uri(Constants.ApiTestUriLink);

            var foundAudienceUrl = _uow.Urls.Get(new QueryExpression<tbl_Urls>()
                .Where(x => x.AudienceId == foundAudience.Id
                    && x.UrlHost == (url.Scheme + "://" + url.Host)
                    && x.UrlPath == url.AbsolutePath).ToLambda())
                .SingleOrDefault();

            if (foundAudienceUrl == null)
            {
                foundAudienceUrl = _uow.Urls.Create(
                    _mapper.Map<tbl_Urls>(new UrlV1()
                    {
                        AudienceId = foundAudience.Id,
                        UrlHost = url.Scheme + "://" + url.Host,
                        UrlPath = url.AbsolutePath,
                        Enabled = true,
                    }));

                _uow.Commit();
            }

            /*
             * create test claims
             */
            var foundClaim = _uow.Claims.Get(new QueryExpression<tbl_Claims>()
                .Where(x => x.Type == Constants.ApiTestClaim).ToLambda())
                .SingleOrDefault();

            if (foundClaim == null)
            {
                foundClaim = _uow.Claims.Create(
                    _mapper.Map<tbl_Claims>(new ClaimV1()
                    {
                        IssuerId = foundIssuer.Id,
                        Subject = Constants.ApiTestClaimSubject,
                        Type = Constants.ApiTestClaim,
                        Value = AlphaNumeric.CreateString(8),
                        ValueType = Constants.ApiTestClaimValueType,
                        Immutable = false,
                    }));

                _uow.Commit();
            }

            /*
             * create test logins
             */
            var foundLogin = _uow.Logins.Get(new QueryExpression<tbl_Logins>()
                .Where(x => x.Name == Constants.ApiTestLogin).ToLambda())
                .SingleOrDefault();

            if (foundLogin == null)
            {
                foundLogin = _uow.Logins.Create(
                    _mapper.Map<tbl_Logins>(new LoginV1()
                    {
                        Name = Constants.ApiTestLogin,
                        Immutable = false,
                    }));

                _uow.Commit();
            }

            /*
             * create test roles
             */
            var foundRole = _uow.Roles.Get(new QueryExpression<tbl_Roles>()
                .Where(x => x.Name == Constants.ApiTestRole).ToLambda())
                .SingleOrDefault();

            if (foundRole == null)
            {
                foundRole = _uow.Roles.Create(
                    _mapper.Map<tbl_Roles>(new RoleV1()
                    {
                        AudienceId = foundAudience.Id,
                        Name = Constants.ApiTestRole,
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Commit();
            }

            /*
             * create test users
             */
            var foundUser = _uow.Users.Get(new QueryExpression<tbl_Users>()
                .Where(x => x.Email == Constants.ApiTestUser).ToLambda())
                .SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = _uow.Users.Create(
                    _mapper.Map<tbl_Users>(new UserV1()
                    {
                        Email = Constants.ApiTestUser,
                        PhoneNumber = Constants.ApiTestUserPhone,
                        FirstName = "First-" + AlphaNumeric.CreateString(4),
                        LastName = "Last-" + AlphaNumeric.CreateString(4),
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = false,
                    }));

                _uow.Activities.Create(
                    _mapper.Map<tbl_Activities>(new ActivityV1()
                    {
                        AudienceId = foundAudience.Id,
                        UserId = foundUser.Id,
                        ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                        Immutable = false,
                    }));

                _uow.Refreshes.Create(
                    _mapper.Map<tbl_Refreshes>(new RefreshV1()
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
                    _mapper.Map<tbl_States>(new StateV1()
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
                    _mapper.Map<tbl_States>(new StateV1()
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

            _uow.Commit();
        }

        public void Destroy()
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            /*
             * delete test users
             */
            _uow.Users.Delete(new QueryExpression<tbl_Users>()
                .Where(x => x.Email.Contains(Constants.ApiTestUser)).ToLambda());
            _uow.Commit();

            /*
             * delete test roles
             */
            _uow.Roles.Delete(new QueryExpression<tbl_Roles>()
                .Where(x => x.Name.Contains(Constants.ApiTestRole)).ToLambda());
            _uow.Commit();

            /*
             * delete test logins
             */
            _uow.Logins.Delete(new QueryExpression<tbl_Logins>()
                .Where(x => x.Name.Contains(Constants.ApiTestLogin)).ToLambda());
            _uow.Commit();

            /*
             * delete test claims
             */
            _uow.Claims.Delete(new QueryExpression<tbl_Claims>()
                .Where(x => x.Type.Contains(Constants.ApiTestClaim)).ToLambda());
            _uow.Commit();

            /*
             * delete test audiences
             */
            _uow.Audiences.Delete(new QueryExpression<tbl_Audiences>()
                .Where(x => x.Name.Contains(Constants.ApiTestAudience)).ToLambda());
            _uow.Commit();

            /*
             * delete test issuers
             */
            _uow.Issuers.Delete(new QueryExpression<tbl_Issuers>()
                .Where(x => x.Name.Contains(Constants.ApiTestIssuer)).ToLambda());
            _uow.Commit();
        }
    }
}
