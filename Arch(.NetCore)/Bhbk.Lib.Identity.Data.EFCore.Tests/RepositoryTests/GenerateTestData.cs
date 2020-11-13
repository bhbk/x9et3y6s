using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using System;
using System.Data;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests
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
                    new uvw_Issuer()
                    {
                        Name = Constants.TestIssuer,
                        IssuerKey = Constants.TestIssuerKey,
                        IsEnabled = true,
                        IsDeletable = true,
                    });

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
                    new uvw_Audience()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = Constants.TestAudience,
                        IsLockedOut = false,
                        IsEnabled = true,
                        IsDeletable = true,
                    });

                _uow.Commit();

                _uow.Activities.Create(
                    new uvw_Activity()
                    {
                        AudienceId = foundAudience.Id,
                        ActivityType = LoginType.CreateAudienceAccessTokenV2.ToString(),
                        IsDeletable = true,
                    });

                _uow.Refreshes.Create(
                    new uvw_Refresh()
                    {
                        IssuerId = foundIssuer.Id,
                        AudienceId = foundAudience.Id,
                        RefreshType = RefreshType.Client.ToString(),
                        RefreshValue = Base64.CreateString(8),
                        IssuedUtc = DateTime.UtcNow,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    });

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
                    new uvw_Claim()
                    {
                        IssuerId = foundIssuer.Id,
                        Subject = Constants.TestClaimSubject,
                        Type = Constants.TestClaim,
                        Value = AlphaNumeric.CreateString(8),
                        ValueType = Constants.TestClaimValueType,
                        IsDeletable = true,
                    });

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
                    new uvw_Login()
                    {
                        Name = Constants.TestLogin,
                        LoginKey = Constants.TestLoginKey,
                        IsEnabled = false,
                        IsDeletable = true,
                    });

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
                    new uvw_Role()
                    {
                        AudienceId = foundAudience.Id,
                        Name = Constants.TestRole,
                        IsEnabled = true,
                        IsDeletable = true,
                    });

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
                    new uvw_User()
                    {
                        UserName = Constants.TestUser,
                        EmailAddress = Constants.TestUser,
                        PhoneNumber = NumberAs.CreateString(9),
                        FirstName = "First-" + Base64.CreateString(4),
                        LastName = "Last-" + Base64.CreateString(4),
                        IsLockedOut = false,
                        IsHumanBeing = true,
                        IsDeletable = true,
                    });

                _uow.Commit();

                _uow.Activities.Create(
                    new uvw_Activity()
                    {
                        UserId = foundUser.Id,
                        ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                        IsDeletable = true,
                    });

                _uow.Refreshes.Create(
                    new uvw_Refresh()
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
                    new uvw_State()
                    {
                        IssuerId = foundIssuer.Id,
                        AudienceId = foundAudience.Id,
                        UserId = foundUser.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                        LastPollingUtc = DateTime.UtcNow,
                    });

                _uow.States.Create(
                    new uvw_State()
                    {
                        IssuerId = foundIssuer.Id,
                        AudienceId = foundAudience.Id,
                        UserId = foundUser.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = StateType.User.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                        LastPollingUtc = DateTime.UtcNow,
                    });

                _uow.Commit();
            }
        }

        public void CreateEmail(uint sets)
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            /*
             * create test users
             */
            var foundUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_User>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda())
                .SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = _uow.Users.Create(
                    new uvw_User()
                    {
                        UserName = Constants.TestUser,
                        EmailAddress = Constants.TestUser,
                        PhoneNumber = NumberAs.CreateString(9),
                        FirstName = "First-" + Base64.CreateString(4),
                        LastName = "Last-" + Base64.CreateString(4),
                        IsLockedOut = false,
                        IsHumanBeing = true,
                        IsDeletable = true,
                    });
                _uow.Commit();
            }

            for (int i = 0; i < sets; i++)
            {
                DateTime now = DateTime.UtcNow;

                var result = _uow.EmailQueue.Create(
                    new uvw_EmailQueue()
                    {
                        FromId = foundUser.Id,
                        FromEmail = foundUser.EmailAddress,
                        ToId = foundUser.Id,
                        ToEmail = foundUser.EmailAddress,
                        Subject = "Subject-" + Base64.CreateString(4),
                        Body = "Body-" + Base64.CreateString(32),
                        SendAtUtc = now,
                    });
            }
            _uow.Commit();
        }

        public void CreateText(uint sets)
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            /*
             * create test users
             */
            var foundUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_User>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda())
                .SingleOrDefault();

            if (foundUser == null)
            {
                foundUser = _uow.Users.Create(
                    new uvw_User()
                    {
                        UserName = Constants.TestUser,
                        EmailAddress = Constants.TestUser,
                        PhoneNumber = NumberAs.CreateString(9),
                        FirstName = "First-" + Base64.CreateString(4),
                        LastName = "Last-" + Base64.CreateString(4),
                        IsLockedOut = false,
                        IsHumanBeing = true,
                        IsDeletable = true,
                    });
                _uow.Commit();
            }

            for (int i = 0; i < sets; i++)
            {
                DateTime now = DateTime.UtcNow;

                var result = _uow.TextQueue.Create(
                    new uvw_TextQueue()
                    {
                        FromId = foundUser.Id,
                        FromPhoneNumber = Constants.TestUserPhoneNumber,
                        ToId = foundUser.Id,
                        ToPhoneNumber = Constants.TestUserPhoneNumber,
                        Body = "Body-" + Base64.CreateString(32),
                        SendAtUtc = now,
                    });
            }
            _uow.Commit();
        }

        public void CreateMOTD(uint sets)
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            for (int i = 0; i < sets; i++)
            {
                _uow.MOTDs.Create(
                    new uvw_MOTD()
                    {
                        Id = Guid.NewGuid(),
                        Author = Constants.TestMotdAuthor,
                        Quote = "Quote-" + Base64.CreateString(4),
                        TssLength = 666,
                        TssId = AlphaNumeric.CreateString(8),
                        TssDate = DateTime.UtcNow,
                        TssCategory = "Test Category",
                        TssTitle = "Test Title",
                        TssBackground = "Test Background",
                        TssTags = "tag1,tag2,tag3",
                    });

                _uow.Commit();
            }
        }

        public void Destroy()
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            /*
             * delete test emails
             */
            _uow.EmailQueue.Delete(QueryExpressionFactory.GetQueryExpression<uvw_EmailQueue>().ToLambda());
            _uow.Commit();

            /*
             * delete test texts
             */
            _uow.TextQueue.Delete(QueryExpressionFactory.GetQueryExpression<uvw_TextQueue>().ToLambda());
            _uow.Commit();

            /*
             * delete test motds
             */
            _uow.MOTDs.Delete(QueryExpressionFactory.GetQueryExpression<uvw_MOTD>()
                .Where(x => x.Author.Contains(Constants.TestMotdAuthor)).ToLambda());
            _uow.Commit();

            /*
             * delete test users
             */
            _uow.Users.Delete(QueryExpressionFactory.GetQueryExpression<uvw_User>()
                .Where(x => x.UserName.Contains(Constants.TestUser)).ToLambda());
            _uow.Commit();

            /*
             * delete test roles
             */
            _uow.Roles.Delete(QueryExpressionFactory.GetQueryExpression<uvw_Role>()
                .Where(x => x.Name.Contains(Constants.TestRole)).ToLambda());
            _uow.Commit();

            /*
             * delete test logins
             */
            _uow.Logins.Delete(QueryExpressionFactory.GetQueryExpression<uvw_Login>()
                .Where(x => x.Name.Contains(Constants.TestLogin)).ToLambda());
            _uow.Commit();

            /*
             * delete test claims
             */
            _uow.Claims.Delete(QueryExpressionFactory.GetQueryExpression<uvw_Claim>()
                .Where(x => x.Type.Contains(Constants.TestClaim)).ToLambda());
            _uow.Commit();

            /*
             * delete test audiences
             */
            _uow.Audiences.Delete(QueryExpressionFactory.GetQueryExpression<uvw_Audience>()
                .Where(x => x.Name.Contains(Constants.TestAudience)).ToLambda());
            _uow.Commit();

            /*
             * delete test issuers
             */
            _uow.Issuers.Delete(QueryExpressionFactory.GetQueryExpression<uvw_Issuer>()
                .Where(x => x.Name.Contains(Constants.TestIssuer)).ToLambda());
            _uow.Commit();
        }
    }
}
