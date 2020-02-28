using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.EF6.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EF6.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class RefreshRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Refreshes_CreateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.Refreshes.Create(new uvw_Refreshes());
            });
        }

        [Fact]
        public void Repo_Refreshes_CreateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var issuer = UoW.Issuers.Get(new QueryExpression<uvw_Issuers>()
                .Where(x => x.Name == Constants.ApiTestIssuer).ToLambda())
                .Single();

            var audience = UoW.Audiences.Get(new QueryExpression<uvw_Audiences>()
                .Where(x => x.Name == Constants.ApiTestAudience).ToLambda())
                .Single();

            var user = UoW.Users.Get(new QueryExpression<uvw_Users>()
                .Where(x => x.Email == Constants.ApiTestUser).ToLambda())
                .Single();

            var result = UoW.Refreshes.Create(
                new uvw_Refreshes()
                {
                    IssuerId = issuer.Id,
                    AudienceId = audience.Id,
                    UserId = user.Id,
                    RefreshType = RefreshType.User.ToString(),
                    RefreshValue = Base64.CreateString(8),
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                });
            result.Should().BeAssignableTo<uvw_Refreshes>();
        }

        [Fact]
        public void Repo_Refreshes_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Refreshes.Delete(new uvw_Refreshes());
            });
        }

        [Fact]
        public void Repo_Refreshes_DeleteV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var activity = UoW.Refreshes.Get().First();

            UoW.Refreshes.Delete(activity);
        }

        [Fact]
        public void Repo_Refreshes_GetV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var results = UoW.Refreshes.Get();
            results.Should().BeAssignableTo<IEnumerable<uvw_Refreshes>>();
            results.Count().Should().Be(UoW.Refreshes.Count());
        }

        [Fact]
        public void Repo_Refreshes_UpdateV1_Fail()
        {
            Assert.Throws<NotImplementedException>(() =>
            {
                UoW.Refreshes.Update(new uvw_Refreshes());
            });
        }
    }
}
