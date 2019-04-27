using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
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
    public class RefreshRepositoryTests
    {
        private StartupTests _factory;

        public RefreshRepositoryTests(StartupTests factory) => _factory = factory;

        [Fact(Skip = "NotImplemented")]
        public async Task Lib_RefreshRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await _factory.UoW.RefreshRepo.CreateAsync(new RefreshCreate());
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await _factory.UoW.RefreshRepo.CreateAsync(
                    new RefreshCreate()
                    {
                        IssuerId = Guid.NewGuid(),
                        ClientId = Guid.NewGuid(),
                        UserId = Guid.NewGuid(),
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = RandomValues.CreateBase64String(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    });
            });
        }

        [Fact]
        public async Task Lib_RefreshRepo_CreateV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();
            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).Single();
            var user = (await _factory.UoW.UserRepo.GetAsync(x => x.Email == Strings.ApiUnitTestUser)).Single();

            var result = await _factory.UoW.RefreshRepo.CreateAsync(
                new RefreshCreate()
                {
                    IssuerId = issuer.Id,
                    ClientId = client.Id,
                    UserId = user.Id,
                    RefreshType = RefreshType.User.ToString(),
                    RefreshValue = RandomValues.CreateBase64String(8),
                    ValidFromUtc = DateTime.UtcNow,
                    ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                });
            result.Should().BeAssignableTo<tbl_Refreshes>();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_RefreshRepo_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _factory.UoW.RefreshRepo.DeleteAsync(Guid.NewGuid());
            });
        }

        [Fact]
        public async Task Lib_RefreshRepo_DeleteV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var refresh = (await _factory.UoW.RefreshRepo.GetAsync()).First();

            var result = await _factory.UoW.RefreshRepo.DeleteAsync(refresh.Id);
            result.Should().BeTrue();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_RefreshRepo_GetV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var result = await _factory.UoW.RefreshRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Refreshes>>();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_RefreshRepo_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<NotImplementedException>(async () =>
            {
                await _factory.UoW.RefreshRepo.UpdateAsync(new tbl_Refreshes());
            });
        }
    }
}
