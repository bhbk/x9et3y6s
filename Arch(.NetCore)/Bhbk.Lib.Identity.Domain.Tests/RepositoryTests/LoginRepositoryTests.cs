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
    public class LoginRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async ValueTask Repo_Logins_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.Logins.CreateAsync(
                    Mapper.Map<tbl_Logins>(new LoginCreate()));
                await UoW.CommitAsync();
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.Logins.CreateAsync(
                    Mapper.Map<tbl_Logins>(new LoginCreate()
                    {
                        Name = Constants.ApiTestLogin,
                        Immutable = false,
                    }));
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Logins_CreateV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var result = await UoW.Logins.CreateAsync(
                Mapper.Map<tbl_Logins>(new LoginCreate()
                {
                    Name = Constants.ApiTestLogin,
                    Immutable = false,
                }));
            result.Should().BeAssignableTo<tbl_Logins>();

            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Logins_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            {
                await UoW.Logins.DeleteAsync(new tbl_Logins());
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Logins_DeleteV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var login = (await UoW.Logins.GetAsync(x => x.Name == Constants.ApiTestLogin)).Single();

            await UoW.Logins.DeleteAsync(login);
            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Logins_GetV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var result = await UoW.Logins.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Logins>>();

            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Logins_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.Logins.UpdateAsync(new tbl_Logins());
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Logins_UpdateV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var login = (await UoW.Logins.GetAsync(x => x.Name == Constants.ApiTestLogin)).Single();
            login.Name += "(Updated)";

            var result = await UoW.Logins.UpdateAsync(login);
            result.Should().BeAssignableTo<tbl_Logins>();

            await UoW.CommitAsync();
        }
    }
}
