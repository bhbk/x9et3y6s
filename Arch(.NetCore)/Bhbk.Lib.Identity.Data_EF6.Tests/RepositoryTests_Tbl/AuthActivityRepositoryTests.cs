using Bhbk.Lib.Identity.Data_EF6.Models_Tbl;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.Identity.Primitives.Tests.Constants;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data_EF6.Tests.RepositoryTests_Tbl
{
    [Collection("RepositoryTests_Tbl")]
    public class AuthActivityRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_AuthActivity_CreateV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.AuthActivity.Delete(new tbl_AuthActivity());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_AuthActivity_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateAudiences();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                .Where(x => x.Name == TestDefaultConstants.AudienceName).ToLambda())
                .Single();

            var result = UoW.AuthActivity.Create(
                Mapper.Map<tbl_AuthActivity>(new AuthActivityV1()
                {
                    AudienceId = audience.Id,
                    LoginType = GrantFlowType.ClientCredentialV2.ToString(),
                    LoginOutcome = GrantFlowResultType.Success.ToString(),
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_AuthActivity>();
        }

        [Fact]
        public void Repo_AuthActivity_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.AuthActivity.Delete(new tbl_AuthActivity());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_AuthActivity_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateAudiences();

            var activity = UoW.AuthActivity.Get(QueryExpressionFactory.GetQueryExpression<tbl_AuthActivity>().ToLambda())
                .First();

            UoW.AuthActivity.Delete(activity);
            UoW.Commit();
        }

        [Fact]
        public void Repo_AuthActivity_GetV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateAudiences();

            var results = UoW.AuthActivity.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_AuthActivity>>();
            results.Count().Should().Be(UoW.AuthActivity.Count());
        }

        [Fact]
        public void Repo_AuthActivity_UpdateV1_Fail()
        {
            Assert.Throws<NotImplementedException>(() =>
            {
                UoW.AuthActivity.Update(new tbl_AuthActivity());
                UoW.Commit();
            });
        }
    }
}
