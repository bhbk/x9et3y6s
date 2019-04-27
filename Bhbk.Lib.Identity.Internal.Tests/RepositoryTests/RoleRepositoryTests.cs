using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Lib.Identity.Internal.Tests.RepositoryTests
{
    [Collection("LibraryTests")]
    public class RoleRepositoryTests
    {
        private StartupTests _factory;

        public RoleRepositoryTests(StartupTests factory) => _factory = factory;

        [Fact(Skip = "NotImplemented")]
        public async Task Lib_RoleRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await _factory.UoW.RoleRepo.CreateAsync(new RoleCreate());
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await _factory.UoW.RoleRepo.CreateAsync(
                    new RoleCreate()
                    {
                        ClientId = Guid.NewGuid(),
                        Name = Strings.ApiUnitTestRole,
                        Enabled = true,
                        Immutable = false,
                    });
            });
        }

        [Fact]
        public async Task Lib_RoleRepo_CreateV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();

            var result = await _factory.UoW.RoleRepo.CreateAsync(
                new RoleCreate()
                {
                    ClientId = client.Id,
                    Name = Strings.ApiUnitTestRole,
                    Enabled = true,
                    Immutable = false,
                });
            result.Should().BeAssignableTo<tbl_Roles>();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_RoleRepo_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _factory.UoW.RoleRepo.DeleteAsync(Guid.NewGuid());
            });
        }

        [Fact]
        public async Task Lib_RoleRepo_DeleteV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var role = (await _factory.UoW.RoleRepo.GetAsync(x => x.Name == Strings.ApiUnitTestRole)).First();

            var result = await _factory.UoW.RoleRepo.DeleteAsync(role.Id);
            result.Should().BeTrue();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_RoleRepo_GetV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var result = await _factory.UoW.RoleRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Roles>>();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_RoleRepo_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _factory.UoW.RoleRepo.UpdateAsync(new tbl_Roles());
            });
        }

        [Fact]
        public async Task Lib_RoleRepo_UpdateV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var role = (await _factory.UoW.RoleRepo.GetAsync(x => x.Name == Strings.ApiUnitTestRole)).First();
            role.Name += "(Updated)";

            var result = await _factory.UoW.RoleRepo.UpdateAsync(role);
            result.Should().BeAssignableTo<tbl_Roles>();

            await _factory.TestData.DestroyAsync();
        }
    }
}
