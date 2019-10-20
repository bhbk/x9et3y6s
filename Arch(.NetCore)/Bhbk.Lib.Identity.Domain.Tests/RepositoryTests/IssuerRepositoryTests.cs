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
    public class IssuerRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async ValueTask Repo_Issuers_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.Issuers.CreateAsync(
                    Mapper.Map<tbl_Issuers>(new IssuerCreate()));

                await UoW.CommitAsync();
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.Issuers.CreateAsync(
                    Mapper.Map<tbl_Issuers>(new IssuerCreate()
                        {
                            Name = Constants.ApiTestIssuer,
                            IssuerKey = Constants.ApiTestIssuerKey,
                            Enabled = true,
                            Immutable = false,
                        }));

                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Issuers_CreateV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var result = await UoW.Issuers.CreateAsync(
                Mapper.Map<tbl_Issuers>(new IssuerCreate()
                    {
                        Name = Constants.ApiTestIssuer,
                        IssuerKey = Constants.ApiTestIssuerKey,
                        Enabled = true,
                        Immutable = false,
                    }));
            result.Should().BeAssignableTo<tbl_Issuers>();

            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Issuers_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            {
                await UoW.Issuers.DeleteAsync(new tbl_Issuers());
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Issuers_DeleteV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var issuer = (await UoW.Issuers.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();

            await UoW.Issuers.DeleteAsync(issuer);
            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Issuers_GetV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var result = await UoW.Issuers.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Issuers>>();

            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Issuers_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.Issuers.UpdateAsync(new tbl_Issuers());
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Issuers_UpdateV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var issuer = (await UoW.Issuers.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            issuer.Name += "(Updated)";

            var result = await UoW.Issuers.UpdateAsync(issuer);
            result.Should().BeAssignableTo<tbl_Issuers>();

            await UoW.CommitAsync();
        }
    }
}
