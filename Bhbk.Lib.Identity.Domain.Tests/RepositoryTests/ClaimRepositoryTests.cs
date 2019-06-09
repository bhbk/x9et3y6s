using Bhbk.Lib.Cryptography.Entropy;
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
    [Collection("LibraryRepositoryTests")]
    public class ClaimRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async Task Lib_ClaimRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.ClaimRepo.CreateAsync(
                    Mapper.Map<tbl_Claims>(new ClaimCreate()));
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.ClaimRepo.CreateAsync(
                    Mapper.Map<tbl_Claims>(new ClaimCreate()
                    {
                        IssuerId = Guid.NewGuid(),
                        Type = Constants.ApiTestClaim,
                        Value = Base64.CreateString(8),
                        Immutable = false,
                    }));
            });
        }

        [Fact]
        public async Task Lib_ClaimRepo_CreateV1_Success()
        {
            TestData.DestroyAsync().Wait();
            TestData.CreateAsync().Wait();

            var issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();

            var result = await UoW.ClaimRepo.CreateAsync(
                Mapper.Map<tbl_Claims>(new ClaimCreate()
                {
                    IssuerId = issuer.Id,
                    Type = Constants.ApiTestClaim,
                    Value = Base64.CreateString(8),
                    Immutable = false,
                }));
            result.Should().BeAssignableTo<tbl_Claims>();
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
            TestData.DestroyAsync().Wait();
            TestData.CreateAsync().Wait();

            var claim = (await UoW.ClaimRepo.GetAsync(x => x.Type == Constants.ApiTestClaim)).Single();

            var result = await UoW.ClaimRepo.DeleteAsync(claim.Id);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Lib_ClaimRepo_GetV1_Success()
        {
            TestData.DestroyAsync().Wait();
            TestData.CreateAsync().Wait();

            var result = await UoW.ClaimRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Claims>>();
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
            TestData.DestroyAsync().Wait();
            TestData.CreateAsync().Wait();

            var claim = (await UoW.ClaimRepo.GetAsync(x => x.Type == Constants.ApiTestClaim)).Single();
            claim.Value += "(Updated)";

            var result = await UoW.ClaimRepo.UpdateAsync(claim);
            result.Should().BeAssignableTo<tbl_Claims>();
        }
    }
}
