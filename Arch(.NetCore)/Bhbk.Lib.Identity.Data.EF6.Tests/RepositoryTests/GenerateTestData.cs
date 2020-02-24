using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.EF6.Models;
using Bhbk.Lib.Identity.Data.EF6.Services;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using System;
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

            var foundIssuer = _uow.Context.Set<tbl_Issuers>()
                .Where(x => x.Name == Constants.ApiTestIssuer)
                .SingleOrDefault();

            if (foundIssuer == null)
            {
                foundIssuer = _uow.Context.Set<tbl_Issuers>().Add(
                    _mapper.Map<tbl_Issuers>(new IssuerCreate()
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

            var foundAudience = _uow.Context.Set<tbl_Audiences>()
                .Where(x => x.Name == Constants.ApiTestAudience)
                .SingleOrDefault();

            if (foundAudience == null)
            {
                foundAudience = _uow.Context.Set<tbl_Audiences>().Add(
                    _mapper.Map<tbl_Audiences>(new AudienceCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        Name = Constants.ApiTestAudience,
                        AudienceType = AudienceType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    }));

                _uow.Context.Set<tbl_Activities>().Add(
                    _mapper.Map<tbl_Activities>(new ActivityCreate()
                    {
                        AudienceId = foundAudience.Id,
                        ActivityType = LoginType.CreateAudienceAccessTokenV2.ToString(),
                        Immutable = false,
                    }));

                _uow.Context.Set<tbl_Refreshes>().Add(
                    _mapper.Map<tbl_Refreshes>(new RefreshCreate()
                    {
                        IssuerId = foundIssuer.Id,
                        AudienceId = foundAudience.Id,
                        RefreshType = RefreshType.Client.ToString(),
                        RefreshValue = Base64.CreateString(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));

                _uow.Commit();
            }
        }

        public void Destroy()
        {
            if (_uow.InstanceType != InstanceContext.UnitTest)
                throw new InvalidOperationException();
        }
    }
}
