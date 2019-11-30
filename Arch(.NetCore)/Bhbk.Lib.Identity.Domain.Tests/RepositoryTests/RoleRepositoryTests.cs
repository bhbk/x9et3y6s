using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Domain.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class RoleRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public void Repo_Roles_CreateV1_Fail()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                UoW.Roles.Create(
                    Mapper.Map<tbl_Roles>(new RoleCreate()));
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Roles_CreateV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var client = UoW.Audiences.Get(x => x.Name == Constants.ApiTestClient).Single();

            var result = UoW.Roles.Create(
                Mapper.Map<tbl_Roles>(new RoleCreate()
                    {
                        AudienceId = client.Id,
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
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var role = UoW.Roles.Get(x => x.Name == Constants.ApiTestRole).Single();

            UoW.Roles.Delete(role);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Roles_GetV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var results = UoW.Roles.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Roles>>();
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
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var role = UoW.Roles.Get(x => x.Name == Constants.ApiTestRole).Single();
            role.Name += "(Updated)";

            var result = UoW.Roles.Update(role);
            result.Should().BeAssignableTo<tbl_Roles>();
            UoW.Commit();
        }
    }
}
