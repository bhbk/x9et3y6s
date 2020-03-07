using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests_DIRECT
{
    [Collection("RepositoryTests_DIRECT")]
    public class RoleRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public void Repo_Roles_CreateV1_Fail()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                UoW.Roles.Create(
                    Mapper.Map<tbl_Roles>(new RoleV1()));
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Roles_CreateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audiences>()
                .Where(x => x.Name == Constants.ApiTestAudience).ToLambda())
                .Single();

            var result = UoW.Roles.Create(
                Mapper.Map<tbl_Roles>(new RoleV1()
                {
                    AudienceId = audience.Id,
                    Name = Constants.ApiTestRole,
                    Enabled = true,
                    Immutable = false,
                }));
            result.Should().BeAssignableTo<tbl_Roles>();

            UoW.Commit();
        }

        [Fact]
        public void Repo_Roles_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Roles.Delete(new tbl_Roles());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Roles_DeleteV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var role = UoW.Roles.Get(QueryExpressionFactory.GetQueryExpression<tbl_Roles>()
                .Where(x => x.Name == Constants.ApiTestRole).ToLambda())
                .Single();

            UoW.Roles.Delete(role);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Roles_GetV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var results = UoW.Roles.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Roles>>();
            results.Count().Should().Be(UoW.Roles.Count());
        }

        [Fact]
        public void Repo_Roles_UpdateV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Roles.Update(new tbl_Roles());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Roles_UpdateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var role = UoW.Roles.Get(QueryExpressionFactory.GetQueryExpression<tbl_Roles>()
                .Where(x => x.Name == Constants.ApiTestRole).ToLambda())
                .Single();
            role.Name += "(Updated)";

            var result = UoW.Roles.Update(role);
            result.Should().BeAssignableTo<tbl_Roles>();

            UoW.Commit();
        }
    }
}
