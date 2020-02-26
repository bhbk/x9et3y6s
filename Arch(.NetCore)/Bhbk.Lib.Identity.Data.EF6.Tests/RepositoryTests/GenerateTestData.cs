using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.EF6.Models;
using Bhbk.Lib.Identity.Data.EF6.Services;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using System;
using System.Data;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.EF6.Tests.RepositoryTests
{
    public class GenerateTestData
    {
        private readonly IUoWService _uow;
        private readonly IMapper _mapper;

        public GenerateTestData(IUoWService uow, IMapper mapper)
        {
            _uow = uow ?? throw new ArgumentNullException();
            _mapper = mapper ?? throw new ArgumentNullException();
        }

        public void Create()
        {
            if (_uow.InstanceType != InstanceContext.UnitTest)
                throw new InvalidOperationException();

            /*
             * create test issuers
             */

            var foundIssuer = _uow.Context.Set<uvw_Issuers>()
                .Where(x => x.Name == Constants.ApiTestIssuer)
                .SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = _uow.Issuers.Create(
                    new uvw_Issuers()
                    {
                        Name = Constants.ApiTestIssuer,
                        IssuerKey = Constants.ApiTestIssuerKey,
                        Created = DateTime.Now,
                        Enabled = true,
                        Immutable = false,
                    });

                _uow.Commit();
            }

            /*
             * create test audiences
             */

            var foundAudience = _uow.Context.Set<uvw_Audiences>()
                .Where(x => x.Name == Constants.ApiTestAudience)
                .SingleOrDefault();

            if (foundAudience == null)
            {
                foundAudience = _uow.Audiences.Create(
                    new uvw_Audiences()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = Constants.ApiTestAudience,
                        AudienceType = AudienceType.user_agent.ToString(),
                        Created = DateTime.Now,
                        Enabled = true,
                        Immutable = false,
                    });

                _uow.Activities.Create(
                    new uvw_Activities()
                    {
                        AudienceId = foundAudience.Id,
                        ActivityType = LoginType.CreateAudienceAccessTokenV2.ToString(),
                        Created = DateTime.Now,
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

                _uow.Commit();
            }
        }

        public void Destroy()
        {
            if (_uow.InstanceType != InstanceContext.UnitTest)
                throw new InvalidOperationException();

            /*
             * delete test users
             */

            var users = _uow.Context.Set<uvw_Users>()
                .Where(x => x.Email.Contains(Constants.ApiTestUser)).ToList();

            foreach (var user in users)
                _uow.Context.usp_User_Delete(user.Id);

            /*
             * delete test audiences
             */

            var audiences = _uow.Context.Set<uvw_Audiences>()
                .Where(x => x.Name.Contains(Constants.ApiTestAudience)).ToList();

            foreach (var audience in audiences)
                _uow.Context.usp_Audience_Delete(audience.Id);

            /*
             * delete test issuers
             */

            var issuers = _uow.Context.Set<uvw_Issuers>()
                .Where(x => x.Name.Contains(Constants.ApiTestIssuer)).ToList();

            foreach (var issuer in issuers)
                _uow.Context.usp_Issuer_Delete(issuer.Id);
        }
    }
}
