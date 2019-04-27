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
    public class LoginRepositoryTests
    {
        private StartupTests _factory;

        public LoginRepositoryTests(StartupTests factory) => _factory = factory;

        [Fact(Skip = "NotImplemented")]
        public async Task Lib_LoginRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await _factory.UoW.LoginRepo.CreateAsync(new LoginCreate());
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await _factory.UoW.LoginRepo.CreateAsync(
                    new LoginCreate()
                    {
                        Name = Strings.ApiUnitTestLogin,
                        Immutable = false,
                    });
            });
        }

        [Fact]
        public async Task Lib_LoginRepo_CreateV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var result = await _factory.UoW.LoginRepo.CreateAsync(
                new LoginCreate()
                {
                    Name = Strings.ApiUnitTestLogin,
                    Immutable = false,
                });
            result.Should().BeAssignableTo<tbl_Logins>();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_LoginRepo_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _factory.UoW.LoginRepo.DeleteAsync(Guid.NewGuid());
            });
        }

        [Fact]
        public async Task Lib_LoginRepo_DeleteV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var login = (await _factory.UoW.LoginRepo.GetAsync(x => x.Name == Strings.ApiUnitTestLogin)).First();

            var result = await _factory.UoW.LoginRepo.DeleteAsync(login.Id);
            result.Should().BeTrue();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_LoginRepo_GetV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var result = await _factory.UoW.LoginRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Logins>>();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_LoginRepo_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _factory.UoW.LoginRepo.UpdateAsync(new tbl_Logins());
            });
        }

        [Fact]
        public async Task Lib_LoginRepo_UpdateV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var login = (await _factory.UoW.LoginRepo.GetAsync(x => x.Name == Strings.ApiUnitTestLogin)).First();
            login.Name += "(Updated)";

            var result = await _factory.UoW.LoginRepo.UpdateAsync(login);
            result.Should().BeAssignableTo<tbl_Logins>();

            await _factory.TestData.DestroyAsync();
        }
    }
}
