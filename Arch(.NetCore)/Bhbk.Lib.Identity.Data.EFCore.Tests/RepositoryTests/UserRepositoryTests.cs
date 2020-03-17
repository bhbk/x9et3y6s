using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class UserRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Users_CreateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.Users.Create(new uvw_Users());
            });
        }

        [Fact]
        public void Repo_Users_CreateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var result = UoW.Users.Create(
                new uvw_Users()
                {
                    UserName = Constants.TestUser,
                    EmailAddress = Constants.TestUser,
                    PhoneNumber = NumberAs.CreateString(9),
                    FirstName = "First-" + Base64.CreateString(4),
                    LastName = "Last-" + Base64.CreateString(4),
                    LockoutEnabled = false,
                    HumanBeing = true,
                    Immutable = false,
                });
            result.Should().BeAssignableTo<uvw_Users>();
        }

        [Fact]
        public void Repo_Users_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Users.Delete(new uvw_Users());
            });
        }

        [Fact]
        public void Repo_Users_DeleteV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_Users>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda()).Single();

            UoW.Users.Delete(user);
        }

        [Fact]
        public void Repo_Users_GetV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var results = UoW.Users.Get();
            results.Should().BeAssignableTo<IEnumerable<uvw_Users>>();
            results.Count().Should().Be(UoW.Users.Count());
        }

        [Fact]
        public void Repo_Users_UpdateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.Users.Update(new uvw_Users());
            });
        }

        [Fact]
        public void Repo_Users_UpdateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_Users>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda()).Single();
            user.FirstName += "(Updated)";
            user.LastName += "(Updated)";

            var result = UoW.Users.Update(user);
            result.Should().BeAssignableTo<uvw_Users>();
        }
    }
}
