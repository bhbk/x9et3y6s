using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Tests.Primitives;
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
    [Collection("LibraryTestsCollection")]
    public class RefreshRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async Task Lib_RefreshRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.RefreshRepo.CreateAsync(
                    UoW.Mapper.Map<tbl_Refreshes>(new RefreshCreate()));
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.RefreshRepo.CreateAsync(
                    UoW.Mapper.Map<tbl_Refreshes>(new RefreshCreate()
                        {
                            IssuerId = Guid.NewGuid(),
                            ClientId = Guid.NewGuid(),
                            UserId = Guid.NewGuid(),
                            RefreshType = RefreshType.User.ToString(),
                            RefreshValue = RandomValues.CreateBase64String(8),
                            ValidFromUtc = DateTime.UtcNow,
                            ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                        }));
            });
        }

        [Fact]
        public async Task Lib_RefreshRepo_CreateV1_Success()
        {
            TestData.CreateAsync().Wait();

            var issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            var client = (await UoW.ClientRepo.GetAsync(x => x.Name == Constants.ApiTestClient)).Single();
            var user = (await UoW.UserRepo.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();

            var result = await UoW.RefreshRepo.CreateAsync(
                UoW.Mapper.Map<tbl_Refreshes>(new RefreshCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = RandomValues.CreateBase64String(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));
            result.Should().BeAssignableTo<tbl_Refreshes>();

            TestData.DestroyAsync().Wait();
        }

        [Fact]
        public async Task Lib_RefreshRepo_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.RefreshRepo.DeleteAsync(Guid.NewGuid());
            });
        }

        [Fact]
        public async Task Lib_RefreshRepo_DeleteV1_Success()
        {
            TestData.CreateAsync().Wait();

            var refresh = (await UoW.RefreshRepo.GetAsync()).First();

            var result = await UoW.RefreshRepo.DeleteAsync(refresh.Id);
            result.Should().BeTrue();

            TestData.DestroyAsync().Wait();
        }

        [Fact]
        public async Task Lib_RefreshRepo_GetV1_Success()
        {
            TestData.CreateAsync().Wait();

            var result = await UoW.RefreshRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Refreshes>>();

            TestData.DestroyAsync().Wait();
        }

        [Fact]
        public async Task Lib_RefreshRepo_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<NotImplementedException>(async () =>
            {
                await UoW.RefreshRepo.UpdateAsync(new tbl_Refreshes());
            });
        }
    }
}
