using Bhbk.Lib.Identity.Data_EF6.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.Identity.Primitives.Tests.Constants;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data_EF6.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class AuthActivityRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_AuthActivity_CreateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.AuthActivity.Create(new E_AuthActivity());
                UoW.Commit();
            });
        }
        
        [Fact]
        public void Repo_AuthActivity_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateAudiences();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<E_Audience>()
                .Where(x => x.Name == TestDefaultConstants.AudienceName).ToLambda())
                .Single();

            var result = UoW.AuthActivity.Create(
                Mapper.Map<E_AuthActivity>(new AuthActivityV1()
                {
                    AudienceId = audience.Id,
                    LoginType = GrantFlowType.ClientCredentialV2.ToString(),
                    LoginOutcome = GrantFlowResultType.Success.ToString(),
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<E_AuthActivity>();
        }

        [Fact]
        public void Repo_AuthActivity_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.AuthActivity.Delete(new E_AuthActivity());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_AuthActivity_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateAudiences();

            var activity = UoW.AuthActivity.Get(QueryExpressionFactory.GetQueryExpression<E_AuthActivity>().ToLambda())
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
            results.Should().BeAssignableTo<IEnumerable<E_AuthActivity>>();
            results.Count().Should().Be(UoW.AuthActivity.Count());
        }

        [Fact]
        public void Repo_AuthActivity_UpdateV1_Fail()
        {
            Assert.Throws<NotImplementedException>(() =>
            {
                UoW.AuthActivity.Update(new E_AuthActivity());
                UoW.Commit();
            });
        }
    }
}
