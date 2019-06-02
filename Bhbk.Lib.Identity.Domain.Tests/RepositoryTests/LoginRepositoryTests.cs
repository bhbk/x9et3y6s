using Bhbk.Lib.Identity.Data.Models;
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
    [Collection("LibraryTestsCollection")]
    public class LoginRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async Task Lib_LoginRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.LoginRepo.CreateAsync(
                    Mapper.Map<tbl_Logins>(new LoginCreate()));
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.LoginRepo.CreateAsync(
                    Mapper.Map<tbl_Logins>(new LoginCreate()
                    {
                        Name = Constants.ApiTestLogin,
                        Immutable = false,
                    }));
            });
        }

        [Fact]
        public async Task Lib_LoginRepo_CreateV1_Success()
        {
            TestData.CreateAsync().Wait();

            var result = await UoW.LoginRepo.CreateAsync(
                Mapper.Map<tbl_Logins>(new LoginCreate()
                {
                    Name = Constants.ApiTestLogin,
                    Immutable = false,
                }));
            result.Should().BeAssignableTo<tbl_Logins>();

            TestData.DestroyAsync().Wait();
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
            TestData.CreateAsync().Wait();

            var login = (await UoW.LoginRepo.GetAsync(x => x.Name == Constants.ApiTestLogin)).Single();

            var result = await UoW.LoginRepo.DeleteAsync(login.Id);
            result.Should().BeTrue();

            TestData.DestroyAsync().Wait();
        }

        [Fact]
        public async Task Lib_LoginRepo_GetV1_Success()
        {
            TestData.CreateAsync().Wait();

            var result = await UoW.LoginRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Logins>>();

            TestData.DestroyAsync().Wait();
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
            TestData.CreateAsync().Wait();

            var login = (await UoW.LoginRepo.GetAsync(x => x.Name == Constants.ApiTestLogin)).Single();
            login.Name += "(Updated)";

            var result = await UoW.LoginRepo.UpdateAsync(login);
            result.Should().BeAssignableTo<tbl_Logins>();

            TestData.DestroyAsync().Wait();
        }
    }
}
