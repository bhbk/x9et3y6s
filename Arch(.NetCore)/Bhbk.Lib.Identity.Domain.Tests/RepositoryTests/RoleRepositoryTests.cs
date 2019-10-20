using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Lib.Identity.Domain.Tests.RepositoryTests
{
    [Collection("LibraryRepositoryTests")]
    public class RoleRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async ValueTask Repo_Roles_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.Roles.CreateAsync(
                    Mapper.Map<tbl_Roles>(new RoleCreate()));
                await UoW.CommitAsync();
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.Roles.CreateAsync(
                    Mapper.Map<tbl_Roles>(new RoleCreate()
                        {
                            ClientId = Guid.NewGuid(),
                            Name = Constants.ApiTestRole,
                            Enabled = true,
                            Immutable = false,
                        }));
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Roles_CreateV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var client = (await UoW.Clients.GetAsync(x => x.Name == Constants.ApiTestClient)).Single();

            var result = await UoW.Roles.CreateAsync(
                Mapper.Map<tbl_Roles>(new RoleCreate()
                    {
                        ClientId = client.Id,
                        Name = Constants.ApiTestRole,
                        Enabled = true,
                        Immutable = false,
                    }));
            result.Should().BeAssignableTo<tbl_Roles>();
            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Roles_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            {
                await UoW.Roles.DeleteAsync(new tbl_Roles());
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Roles_DeleteV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var role = (await UoW.Roles.GetAsync(x => x.Name == Constants.ApiTestRole)).Single();

            await UoW.Roles.DeleteAsync(role);
            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Roles_GetV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var result = await UoW.Roles.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Roles>>();
            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Roles_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.Roles.UpdateAsync(new tbl_Roles());
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Roles_UpdateV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var role = (await UoW.Roles.GetAsync(x => x.Name == Constants.ApiTestRole)).Single();
            role.Name += "(Updated)";

            var result = await UoW.Roles.UpdateAsync(role);
            result.Should().BeAssignableTo<tbl_Roles>();
            await UoW.CommitAsync();
        }
    }
}
