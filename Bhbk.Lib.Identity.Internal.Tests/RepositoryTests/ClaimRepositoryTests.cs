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
    [Collection("LibraryTestsCollection")]
    public class ClaimRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async Task Lib_ClaimRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.ClaimRepo.CreateAsync(
                    UoW.Mapper.Map<tbl_Claims>(new ClaimCreate()));
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.ClaimRepo.CreateAsync(
                    UoW.Mapper.Map<tbl_Claims>(new ClaimCreate()
                    {
                        IssuerId = Guid.NewGuid(),
                        Type = Constants.ApiUnitTestClaim,
                        Value = RandomValues.CreateBase64String(8),
                        Immutable = false,
                    }));
            });
        }

        [Fact]
        public async Task Lib_ClaimRepo_CreateV1_Success()
        {
            TestData.CreateAsync().Wait();

            var issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).First();

            var result = await UoW.ClaimRepo.CreateAsync(
                UoW.Mapper.Map<tbl_Claims>(new ClaimCreate()
                {
                    IssuerId = issuer.Id,
                    Type = Constants.ApiUnitTestClaim,
                    Value = RandomValues.CreateBase64String(8),
                    Immutable = false,
                }));
            result.Should().BeAssignableTo<tbl_Claims>();

            TestData.DestroyAsync().Wait();
        }

        [Fact]
        public async Task Lib_ClaimRepo_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.ClaimRepo.DeleteAsync(Guid.NewGuid());
            });
        }

        [Fact]
        public async Task Lib_ClaimRepo_DeleteV1_Success()
        {
            TestData.CreateAsync().Wait();

            var claim = (await UoW.ClaimRepo.GetAsync(x => x.Type == Constants.ApiUnitTestClaim)).First();

            var result = await UoW.ClaimRepo.DeleteAsync(claim.Id);
            result.Should().BeTrue();

            TestData.DestroyAsync().Wait();
        }

        [Fact]
        public async Task Lib_ClaimRepo_GetV1_Success()
        {
            TestData.CreateAsync().Wait();

            var result = await UoW.ClaimRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Claims>>();

            TestData.DestroyAsync().Wait();
        }

        [Fact]
        public async Task Lib_ClaimRepo_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.ClaimRepo.UpdateAsync(new tbl_Claims());
            });
        }

        [Fact]
        public async Task Lib_ClaimRepo_UpdateV1_Success()
        {
            TestData.CreateAsync().Wait();

            var claim = (await UoW.ClaimRepo.GetAsync(x => x.Type == Constants.ApiUnitTestClaim)).First();
            claim.Value += "(Updated)";

            var result = await UoW.ClaimRepo.UpdateAsync(claim);
            result.Should().BeAssignableTo<tbl_Claims>();

            TestData.DestroyAsync().Wait();
        }
    }
}
