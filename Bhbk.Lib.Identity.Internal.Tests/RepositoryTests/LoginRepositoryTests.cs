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
    public class LoginRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async Task Lib_LoginRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.LoginRepo.CreateAsync(new LoginCreate());
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.LoginRepo.CreateAsync(
                    new LoginCreate()
                    {
                        Name = Constants.ApiUnitTestLogin,
                        Immutable = false,
                    });
            });
        }

        [Fact]
        public async Task Lib_LoginRepo_CreateV1_Success()
        {
            await TestData.CreateAsync();

            var result = await UoW.LoginRepo.CreateAsync(
                new LoginCreate()
                {
                    Name = Constants.ApiUnitTestLogin,
                    Immutable = false,
                });
            result.Should().BeAssignableTo<tbl_Logins>();

            await TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_LoginRepo_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.LoginRepo.DeleteAsync(Guid.NewGuid());
            });
        }

        [Fact]
        public async Task Lib_LoginRepo_DeleteV1_Success()
        {
            await TestData.CreateAsync();

            var login = (await UoW.LoginRepo.GetAsync(x => x.Name == Constants.ApiUnitTestLogin)).First();

            var result = await UoW.LoginRepo.DeleteAsync(login.Id);
            result.Should().BeTrue();

            await TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_LoginRepo_GetV1_Success()
        {
            await TestData.CreateAsync();

            var result = await UoW.LoginRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Logins>>();

            await TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_LoginRepo_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.LoginRepo.UpdateAsync(new tbl_Logins());
            });
        }

        [Fact]
        public async Task Lib_LoginRepo_UpdateV1_Success()
        {
            await TestData.CreateAsync();

            var login = (await UoW.LoginRepo.GetAsync(x => x.Name == Constants.ApiUnitTestLogin)).First();
            login.Name += "(Updated)";

            var result = await UoW.LoginRepo.UpdateAsync(login);
            result.Should().BeAssignableTo<tbl_Logins>();

            await TestData.DestroyAsync();
        }
    }
}
