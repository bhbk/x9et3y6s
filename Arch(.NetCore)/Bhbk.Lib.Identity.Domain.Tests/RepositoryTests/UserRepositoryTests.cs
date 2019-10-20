using Bhbk.Lib.Cryptography.Entropy;
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
    public class UserRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async ValueTask Repo_Users_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.Users.CreateAsync(
                    Mapper.Map<tbl_Users>(new UserCreate()));
                await UoW.CommitAsync();
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.Users.CreateAsync(
                    Mapper.Map<tbl_Users>(new UserCreate()
                        {
                            Email = AlphaNumeric.CreateString(4),
                            PhoneNumber = Constants.ApiTestUserPhone,
                            FirstName = "First-" + Base64.CreateString(4),
                            LastName = "Last-" + Base64.CreateString(4),
                            LockoutEnabled = false,
                            HumanBeing = true,
                            Immutable = false,
                        }));
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Users_CreateV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var result = await UoW.Users.CreateAsync(
                Mapper.Map<tbl_Users>(new UserCreate()
                    {
                        Email = Constants.ApiTestUser,
                        PhoneNumber = Constants.ApiTestUserPhone,
                        FirstName = "First-" + Base64.CreateString(4),
                        LastName = "Last-" + Base64.CreateString(4),
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = false,
                    }));
            result.Should().BeAssignableTo<tbl_Users>();
            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Users_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            {
                await UoW.Users.DeleteAsync(new tbl_Users());
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Users_DeleteV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var issuer = (await UoW.Users.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();

            await UoW.Users.DeleteAsync(issuer);
            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Users_GetV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var result = await UoW.Users.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Users>>();
            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Users_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await UoW.Users.UpdateAsync(new tbl_Users());
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Users_UpdateV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var user = (await UoW.Users.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();
            user.FirstName += "(Updated)";
            user.LastName += "(Updated)";

            var result = await UoW.Users.UpdateAsync(user);

            result.Should().BeAssignableTo<tbl_Users>();
            await UoW.CommitAsync();
        }
    }
}
