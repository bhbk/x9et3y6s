﻿using Bhbk.Lib.Identity.Data_EF6.Models_TSQL;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data_EF6.Tests.RepositoryTests_TSQL
{
    [Collection("RepositoryTests_TSQL")]
    public class RoleRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Roles_CreateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.Roles.Create(new uvw_Role());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Roles_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateRoles();

            var audience = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<uvw_Audience>()
                .Where(x => x.Name == Constants.TestAudience).ToLambda())
                .Single();

            var result = UoW.Roles.Create(
                Mapper.Map<uvw_Role>(new RoleV1()
                {
                    AudienceId = audience.Id,
                    Name = Constants.TestRole,
                    IsEnabled = true,
                    IsDeletable = true,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<uvw_Role>();
        }

        [Fact]
        public void Repo_Roles_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Roles.Delete(new uvw_Role());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Roles_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateRoles();

            var role = UoW.Roles.Get(QueryExpressionFactory.GetQueryExpression<uvw_Role>()
                .Where(x => x.Name == Constants.TestRole).ToLambda())
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
            results.Should().BeAssignableTo<IEnumerable<uvw_Role>>();
            results.Count().Should().Be(UoW.Roles.Count());
        }

        [Fact]
        public void Repo_Roles_UpdateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.Roles.Update(new uvw_Role());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Roles_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateRoles();

            var role = UoW.Roles.Get(QueryExpressionFactory.GetQueryExpression<uvw_Role>()
                .Where(x => x.Name == Constants.TestRole).ToLambda())
                .Single();
            role.Name += "(Updated)";

            var result = UoW.Roles.Update(role);
            UoW.Commit();

            result.Should().BeAssignableTo<uvw_Role>();
        }
    }
}