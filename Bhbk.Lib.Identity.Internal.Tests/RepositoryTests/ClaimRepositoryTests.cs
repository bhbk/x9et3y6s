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
    public class ClaimRepositoryTests
    {
        private StartupTests _factory;

        public ClaimRepositoryTests(StartupTests factory) => _factory = factory;

        [Fact(Skip = "NotImplemented")]
        public async Task Lib_ClaimRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await _factory.UoW.ClaimRepo.CreateAsync(new ClaimCreate());
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await _factory.UoW.ClaimRepo.CreateAsync(
                    new ClaimCreate()
                    {
                        IssuerId = Guid.NewGuid(),
                        Type = Strings.ApiUnitTestClaim,
                        Value = RandomValues.CreateBase64String(8),
                        Immutable = false,
                    });
            });
        }

        [Fact]
        public async Task Lib_ClaimRepo_CreateV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).First();

            var result = await _factory.UoW.ClaimRepo.CreateAsync(
                new ClaimCreate()
                {
                    IssuerId = issuer.Id,
                    Type = Strings.ApiUnitTestClaim,
                    Value = RandomValues.CreateBase64String(8),
                    Immutable = false,
                });
            result.Should().BeAssignableTo<tbl_Claims>();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_ClaimRepo_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _factory.UoW.ClaimRepo.DeleteAsync(Guid.NewGuid());
            });
        }

        [Fact]
        public async Task Lib_ClaimRepo_DeleteV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var claim = (await _factory.UoW.ClaimRepo.GetAsync(x => x.Type == Strings.ApiUnitTestClaim)).First();

            var result = await _factory.UoW.ClaimRepo.DeleteAsync(claim.Id);
            result.Should().BeTrue();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_ClaimRepo_GetV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var result = await _factory.UoW.ClaimRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Claims>>();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_ClaimRepo_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _factory.UoW.ClaimRepo.UpdateAsync(new tbl_Claims());
            });
        }

        [Fact]
        public async Task Lib_ClaimRepo_UpdateV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var claim = (await _factory.UoW.ClaimRepo.GetAsync(x => x.Type == Strings.ApiUnitTestClaim)).First();
            claim.Value += "(Updated)";

            var result = await _factory.UoW.ClaimRepo.UpdateAsync(claim);
            result.Should().BeAssignableTo<tbl_Claims>();

            await _factory.TestData.DestroyAsync();
        }
    }
}
