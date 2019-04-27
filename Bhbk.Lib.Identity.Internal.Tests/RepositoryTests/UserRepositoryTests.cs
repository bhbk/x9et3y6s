using Bhbk.Lib.Core.Cryptography;
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
    public class UserRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async Task Lib_UserRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.UserRepo.CreateAsync(new UserCreate());
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.UserRepo.CreateAsync(
                    new UserCreate()
                    {
                        Email = RandomValues.CreateAlphaNumericString(4),
                        PhoneNumber = Constants.ApiUnitTestUserPhone,
                        FirstName = "First-" + RandomValues.CreateBase64String(4),
                        LastName = "Last-" + RandomValues.CreateBase64String(4),
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = false,
                    });
            });
        }

        [Fact]
        public async Task Lib_UserRepo_CreateV1_Success()
        {
            await TestData.CreateAsync();

            var result = await UoW.UserRepo.CreateAsync(
                new UserCreate()
                {
                    Email = Constants.ApiUnitTestUser,
                    PhoneNumber = Constants.ApiUnitTestUserPhone,
                    FirstName = "First-" + RandomValues.CreateBase64String(4),
                    LastName = "Last-" + RandomValues.CreateBase64String(4),
                    LockoutEnabled = false,
                    HumanBeing = true,
                    Immutable = false,
                });
            result.Should().BeAssignableTo<tbl_Users>();

            await TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_UserRepo_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.UserRepo.DeleteAsync(Guid.NewGuid());
            });
        }

        [Fact]
        public async Task Lib_UserRepo_DeleteV1_Success()
        {
            await TestData.CreateAsync();

            var issuer = (await UoW.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).First();

            var result = await UoW.UserRepo.DeleteAsync(issuer.Id);
            result.Should().BeTrue();

            await TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_UserRepo_GetV1_Success()
        {
            await TestData.CreateAsync();

            var result = await UoW.UserRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Users>>();

            await TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_UserRepo_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await UoW.UserRepo.UpdateAsync(new tbl_Users());
            });
        }

        [Fact]
        public async Task Lib_UserRepo_UpdateV1_Success()
        {
            await TestData.CreateAsync();

            var user = (await UoW.UserRepo.GetAsync(x => x.Email == Constants.ApiUnitTestUser)).First();
            user.FirstName += "(Updated)";
            user.LastName += "(Updated)";

            var result = await UoW.UserRepo.UpdateAsync(user);

            result.Should().BeAssignableTo<tbl_Users>();

            await TestData.DestroyAsync();
        }
    }
}
