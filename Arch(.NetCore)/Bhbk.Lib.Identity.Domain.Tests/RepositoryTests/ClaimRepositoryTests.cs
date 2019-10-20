using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.DataState.Expressions;
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
    public class ClaimRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async ValueTask Repo_Claims_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.Claims.CreateAsync(
                    Mapper.Map<tbl_Claims>(new ClaimCreate()));

                await UoW.CommitAsync();
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.Claims.CreateAsync(
                    Mapper.Map<tbl_Claims>(new ClaimCreate()
                    {
                        IssuerId = Guid.NewGuid(),
                        Type = Constants.ApiTestClaim,
                        Value = Base64.CreateString(8),
                        Immutable = false,
                    }));

                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Claims_CreateV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var issuer = (await UoW.Issuers.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();

            var result = await UoW.Claims.CreateAsync(
                Mapper.Map<tbl_Claims>(new ClaimCreate()
                {
                    IssuerId = issuer.Id,
                    Type = Constants.ApiTestClaim,
                    Value = Base64.CreateString(8),
                    Immutable = false,
                }));
            result.Should().BeAssignableTo<tbl_Claims>();

            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Claims_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            {
                await UoW.Claims.DeleteAsync(new tbl_Claims());
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Claims_DeleteV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var claim = (await UoW.Claims.GetAsync(new QueryExpression<tbl_Claims>()
                .Where(x => x.Type == Constants.ApiTestClaim).ToLambda()))
                .Single();

            await UoW.Claims.DeleteAsync(claim);
            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Claims_GetV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var result = await UoW.Claims.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Claims>>();

            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Claims_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.Claims.UpdateAsync(new tbl_Claims());
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Claims_UpdateV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var claim = (await UoW.Claims.GetAsync(new QueryExpression<tbl_Claims>()
                .Where(x => x.Type == Constants.ApiTestClaim)
                .ToLambda())).Single();
            claim.Value += "(Updated)";

            var result = await UoW.Claims.UpdateAsync(claim);
            result.Should().BeAssignableTo<tbl_Claims>();

            await UoW.CommitAsync();
        }
    }
}
