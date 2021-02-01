using Bhbk.Lib.Identity.Data_EF6.Models;
using Bhbk.Lib.Identity.Models.Admin;
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
    public class RoleRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Roles_CreateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.Roles.Create(new E_Role());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Roles_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateRoles();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<E_Audience>()
                .Where(x => x.Name == TestDefaultConstants.AudienceName).ToLambda())
                .Single();

            var result = UoW.Roles.Create(
                Mapper.Map<E_Role>(new RoleV1()
                {
                    AudienceId = audience.Id,
                    Name = TestDefaultConstants.RoleName,
                    IsEnabled = true,
                    IsDeletable = true,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<E_Role>();
        }

        [Fact]
        public void Repo_Roles_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Roles.Delete(new E_Role());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Roles_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateRoles();

            var role = UoW.Roles.Get(QueryExpressionFactory.GetQueryExpression<E_Role>()
                .Where(x => x.Name == TestDefaultConstants.RoleName).ToLambda())
                .Single();

            UoW.Roles.Delete(role);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Roles_GetV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateRoles();

            var results = UoW.Roles.Get();
            results.Should().BeAssignableTo<IEnumerable<E_Role>>();
            results.Count().Should().Be(UoW.Roles.Count());
        }

        [Fact]
        public void Repo_Roles_UpdateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.Roles.Update(new E_Role());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Roles_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateRoles();

            var role = UoW.Roles.Get(QueryExpressionFactory.GetQueryExpression<E_Role>()
                .Where(x => x.Name == TestDefaultConstants.RoleName).ToLambda())
                .Single();
            role.Name += "(Updated)";

            var result = UoW.Roles.Update(role);
            UoW.Commit();

            result.Should().BeAssignableTo<E_Role>();
        }
    }
}
