using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Domain.Builders;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Models.Admin;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Test.Identity.Integration.Repositories
{
    [Collection("RepositoryTests")]
    public class RoleRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Roles_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateException>(() =>
            {
                UoW.Roles.Post(new tbl_Role());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Roles_CreateV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults())
                .Seed();

            var audience = seed.Audiences.Values.First();

            var result = UoW.Roles.Post(
                Mapper.Map<tbl_Role>(new RoleV1()
                {
                    AudienceId = audience.Id,
                    Name = new RoleBuilder().WithDefaults().GetName(),
                    IsEnabled = true,
                    IsDeletable = true,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Role>();

            UoW.Roles.Delete(result);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Roles_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Roles.Delete(new tbl_Role());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Roles_DeleteV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults())
                .Seed();

            var audience = seed.Audiences.Values.First();

            var role = UoW.Roles.Post(
                Mapper.Map<tbl_Role>(new RoleV1()
                {
                    AudienceId = audience.Id,
                    Name = new RoleBuilder().WithDefaults().GetName(),
                    IsEnabled = true,
                    IsDeletable = true,
                }));
            UoW.Commit();

            UoW.Roles.Delete(role);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Roles_GetV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults())
                .Seed();

            var results = UoW.Roles.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Role>>();
            results.Count().Should().Be(UoW.Roles.Count());
        }

        [Fact]
        public void Repo_Roles_UpdateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Roles.Put(new tbl_Role());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Roles_UpdateV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults().WithStandardRoles())
                .Seed();

            var role = seed.Roles.Values.First();
            role.Name += "(Updated)";

            var result = UoW.Roles.Put(role);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Role>();
        }
    }
}
