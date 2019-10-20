using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
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
    public class RefreshRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async ValueTask Repo_Refreshes_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.Refreshes.CreateAsync(
                    Mapper.Map<tbl_Refreshes>(new RefreshCreate()));
                await UoW.CommitAsync();
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.Refreshes.CreateAsync(
                    Mapper.Map<tbl_Refreshes>(new RefreshCreate()
                        {
                            IssuerId = Guid.NewGuid(),
                            ClientId = Guid.NewGuid(),
                            UserId = Guid.NewGuid(),
                            RefreshType = RefreshType.User.ToString(),
                            RefreshValue = Base64.CreateString(8),
                            ValidFromUtc = DateTime.UtcNow,
                            ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                        }));
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Refreshes_CreateV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var issuer = (await UoW.Issuers.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            var client = (await UoW.Clients.GetAsync(x => x.Name == Constants.ApiTestClient)).Single();
            var user = (await UoW.Users.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();

            var result = await UoW.Refreshes.CreateAsync(
                Mapper.Map<tbl_Refreshes>(new RefreshCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = Base64.CreateString(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));
            result.Should().BeAssignableTo<tbl_Refreshes>();

            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Refreshes_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            {
                await UoW.Refreshes.DeleteAsync(new tbl_Refreshes());
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Refreshes_DeleteV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var refresh = (await UoW.Refreshes.GetAsync()).First();

            await UoW.Refreshes.DeleteAsync(refresh);
            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Refreshes_GetV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var result = await UoW.Refreshes.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Refreshes>>();
        }

        [Fact]
        public async ValueTask Repo_Refreshes_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<NotImplementedException>(async () =>
            {
                await UoW.Refreshes.UpdateAsync(new tbl_Refreshes());
                await UoW.CommitAsync();
            });
        }
    }
}
