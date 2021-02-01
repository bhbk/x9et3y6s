using Bhbk.Lib.Cryptography.Entropy;
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
    public class StateRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_States_CreateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.States.Create(new E_State());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_States_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateUserStates();

            var issuer = UoW.Issuers.Get(QueryExpressionFactory.GetQueryExpression<E_Issuer>()
                .Where(x => x.Name == TestDefaultConstants.IssuerName).ToLambda())
                .Single();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<E_Audience>()
                .Where(x => x.Name == TestDefaultConstants.AudienceName).ToLambda())
                .Single();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<E_User>()
                .Where(x => x.UserName == TestDefaultConstants.UserName).ToLambda())
                .Single();

            var result = UoW.States.Create(
                Mapper.Map<E_State>(new StateV1()
                {
                    IssuerId = issuer.Id,
                    AudienceId = audience.Id,
                    UserId = user.Id,
                    StateValue = AlphaNumeric.CreateString(32),
                    StateType = ConsumerType.Device.ToString(),
                    StateConsume = false,
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<E_State>();
        }

        [Fact]
        public void Repo_States_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.States.Delete(new E_State());
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
            results.Should().BeAssignableTo<IEnumerable<E_State>>();
            results.Count().Should().Be(UoW.States.Count());
        }

        [Fact]
        public void Repo_States_UpdateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.States.Update(new E_State());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_States_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateUserStates();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<E_User>()
                .Where(x => x.UserName == TestDefaultConstants.UserName).ToLambda())
                .Single();

            var state = UoW.States.Get(QueryExpressionFactory.GetQueryExpression<E_State>()
                .Where(x => x.UserId == user.Id).ToLambda())
                .First();

            state.StateConsume = true;

            var result = UoW.States.Update(state);
            UoW.Commit();

            result.Should().BeAssignableTo<E_State>();
        }
    }
}
