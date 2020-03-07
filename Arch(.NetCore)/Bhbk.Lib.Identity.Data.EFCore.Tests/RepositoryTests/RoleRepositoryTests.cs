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
    public class RoleRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Roles_CreateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.Roles.Create(new uvw_Roles());
            });
        }

        [Fact]
        public void Repo_Roles_CreateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<uvw_Audiences>()
                .Where(x => x.Name == Constants.ApiTestAudience).ToLambda())
                .Single();

            var result = UoW.Roles.Create(
                new uvw_Roles()
                {
                    AudienceId = audience.Id,
                    Name = Constants.ApiTestRole,
                    Enabled = true,
                    Immutable = false,
                });
            result.Should().BeAssignableTo<uvw_Roles>();
        }

        [Fact]
        public void Repo_Roles_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Roles.Delete(new uvw_Roles());
            });
        }

        [Fact]
        public void Repo_Roles_DeleteV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var role = UoW.Roles.Get(QueryExpressionFactory.GetQueryExpression<uvw_Roles>()
                .Where(x => x.Name == Constants.ApiTestRole).ToLambda())
                .Single();

            UoW.Roles.Delete(role);
        }

        [Fact]
        public void Repo_Roles_GetV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var results = UoW.Roles.Get();
            results.Should().BeAssignableTo<IEnumerable<uvw_Roles>>();
            results.Count().Should().Be(UoW.Roles.Count());
        }

        [Fact]
        public void Repo_Roles_UpdateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.Roles.Update(new uvw_Roles());
            });
        }

        [Fact]
        public void Repo_Roles_UpdateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var role = UoW.Roles.Get(QueryExpressionFactory.GetQueryExpression<uvw_Roles>()
                .Where(x => x.Name == Constants.ApiTestRole).ToLambda())
                .Single();
            role.Name += "(Updated)";

            var result = UoW.Roles.Update(role);
            result.Should().BeAssignableTo<uvw_Roles>();
        }
    }
}
