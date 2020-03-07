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
    public class LoginRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Logins_CreateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.Logins.Create(new uvw_Logins());
            });
        }

        [Fact]
        public void Repo_Logins_CreateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var result = UoW.Logins.Create(
                new uvw_Logins()
                {
                    Name = Constants.ApiTestLogin,
                    LoginKey = Constants.ApiTestLoginKey,
                    Enabled = true,
                    Immutable = false,
                });
            result.Should().BeAssignableTo<uvw_Logins>();
        }

        [Fact]
        public void Repo_Logins_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Logins.Delete(new uvw_Logins());
            });
        }

        [Fact]
        public void Repo_Logins_DeleteV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var login = UoW.Logins.Get(QueryExpressionFactory.GetQueryExpression<uvw_Logins>()
                .Where(x => x.Name == Constants.ApiTestLogin).ToLambda())
                .Single();

            UoW.Logins.Delete(login);
        }

        [Fact]
        public void Repo_Logins_GetV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var results = UoW.Logins.Get();
            results.Should().BeAssignableTo<IEnumerable<uvw_Logins>>();
            results.Count().Should().Be(UoW.Logins.Count());
        }

        [Fact]
        public void Repo_Logins_UpdateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.Logins.Update(new uvw_Logins());
            });
        }

        [Fact]
        public void Repo_Logins_UpdateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var login = UoW.Logins.Get(QueryExpressionFactory.GetQueryExpression<uvw_Logins>()
                .Where(x => x.Name == Constants.ApiTestLogin).ToLambda())
                .Single();
            login.Name += "(Updated)";

            var result = UoW.Logins.Update(login);
            result.Should().BeAssignableTo<uvw_Logins>();
        }
    }
}
