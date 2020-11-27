using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data_EF6.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
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
    public class StateRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_States_CreateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.States.Create(new uvw_State());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_States_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateUserStates();

            var issuer = UoW.Issuers.Get(QueryExpressionFactory.GetQueryExpression<uvw_Issuer>()
                .Where(x => x.Name == Constants.TestIssuer).ToLambda())
                .Single();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<uvw_Audience>()
                .Where(x => x.Name == Constants.TestAudience).ToLambda())
                .Single();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_User>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda())
                .Single();

            var result = UoW.States.Create(
                Mapper.Map<uvw_State>(new StateV1()
                {
                    IssuerId = issuer.Id,
                    AudienceId = audience.Id,
                    UserId = user.Id,
                    StateValue = AlphaNumeric.CreateString(32),
                    StateType = StateType.Device.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<uvw_State>();
        }

        [Fact]
        public void Repo_States_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.States.Delete(new uvw_State());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_States_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateUserStates();

            var state = UoW.States.Get().First();

            UoW.States.Delete(state);
            UoW.Commit();
        }

        [Fact]
        public void Repo_States_GetV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateUserStates();

            var results = UoW.States.Get();
            results.Should().BeAssignableTo<IEnumerable<uvw_State>>();
            results.Count().Should().Be(UoW.States.Count());
        }

        [Fact]
        public void Repo_States_UpdateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.States.Update(new uvw_State());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_States_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateUserStates();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_User>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda())
                .Single();

            var state = UoW.States.Get(QueryExpressionFactory.GetQueryExpression<uvw_State>()
                .Where(x => x.UserId == user.Id).ToLambda())
                .First();

            state.StateConsume = true;

            var result = UoW.States.Update(state);
            UoW.Commit();

            result.Should().BeAssignableTo<uvw_State>();
        }
    }
}
