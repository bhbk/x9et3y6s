using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Domain.Tests.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Lib.Identity.Domain.Tests.RepositoryTests
{
    [Collection("LibraryTestsCollection")]
    public class RoleRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async Task Lib_RoleRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.RoleRepo.CreateAsync(
                    UoW.Mapper.Map<tbl_Roles>(new RoleCreate()));
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.RoleRepo.CreateAsync(
                    UoW.Mapper.Map<tbl_Roles>(new RoleCreate()
                        {
                            ClientId = Guid.NewGuid(),
                            Name = Constants.ApiTestRole,
                            Enabled = true,
                            Immutable = false,
                        }));
            });
        }

        [Fact]
        public async Task Lib_RoleRepo_CreateV1_Success()
        {
            TestData.DestroyAsync().Wait();
            TestData.CreateAsync().Wait();

            var client = (await UoW.ClientRepo.GetAsync(x => x.Name == Constants.ApiTestClient)).Single();

            var result = await UoW.RoleRepo.CreateAsync(
                UoW.Mapper.Map<tbl_Roles>(new RoleCreate()
                    {
                        ClientId = client.Id,
                        Name = Constants.ApiTestRole,
                        Enabled = true,
                        Immutable = false,
                    }));
            result.Should().BeAssignableTo<tbl_Roles>();
        }

        [Fact]
        public async Task Lib_RoleRepo_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.RoleRepo.DeleteAsync(Guid.NewGuid());
            });
        }

        [Fact]
        public async Task Lib_RoleRepo_DeleteV1_Success()
        {
            TestData.DestroyAsync().Wait();
            TestData.CreateAsync().Wait();

            var role = (await UoW.RoleRepo.GetAsync(x => x.Name == Constants.ApiTestRole)).Single();

            var result = await UoW.RoleRepo.DeleteAsync(role.Id);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Lib_RoleRepo_GetV1_Success()
        {
            TestData.DestroyAsync().Wait();
            TestData.CreateAsync().Wait();

            var result = await UoW.RoleRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Roles>>();
        }

        [Fact]
        public async Task Lib_RoleRepo_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.RoleRepo.UpdateAsync(new tbl_Roles());
            });
        }

        [Fact]
        public async Task Lib_RoleRepo_UpdateV1_Success()
        {
            TestData.DestroyAsync().Wait();
            TestData.CreateAsync().Wait();

            var role = (await UoW.RoleRepo.GetAsync(x => x.Name == Constants.ApiTestRole)).Single();
            role.Name += "(Updated)";

            var result = await UoW.RoleRepo.UpdateAsync(role);
            result.Should().BeAssignableTo<tbl_Roles>();
        }
    }
}
