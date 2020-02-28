using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.EF6.Models;
using Bhbk.Lib.Identity.Primitives;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EF6.Tests.RepositoryTests
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

            var audience = UoW.Audiences.Get(new QueryExpression<uvw_Audiences>()
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

            var role = UoW.Roles.Get(new QueryExpression<uvw_Roles>()
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

        [Fact(Skip = "NotImplemented")]
        public void Repo_Roles_UpdateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var role = UoW.Roles.Get(new QueryExpression<uvw_Roles>()
                .Where(x => x.Name == Constants.ApiTestRole).ToLambda())
                .Single();
            role.Name += "(Updated)";

            var result = UoW.Roles.Update(role);
            result.Should().BeAssignableTo<uvw_Roles>();
        }
    }
}
