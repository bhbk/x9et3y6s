using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Domain.Tests.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Lib.Identity.Domain.Tests.RepositoryTests
{
    [Collection("LibraryTestsCollection")]
    public class IssuerRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async Task Lib_IssuerRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.IssuerRepo.CreateAsync(
                    UoW.Mapper.Map<tbl_Issuers>(new IssuerCreate()));
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.IssuerRepo.CreateAsync(
                    UoW.Mapper.Map<tbl_Issuers>(new IssuerCreate()
                        {
                            Name = Constants.ApiTestIssuer,
                            IssuerKey = Constants.ApiTestIssuerKey,
                            Enabled = true,
                            Immutable = false,
                        }));
            });
        }

        [Fact]
        public async Task Lib_IssuerRepo_CreateV1_Success()
        {
            TestData.CreateAsync().Wait();

            var result = await UoW.IssuerRepo.CreateAsync(
                UoW.Mapper.Map<tbl_Issuers>(new IssuerCreate()
                    {
                        Name = Constants.ApiTestIssuer,
                        IssuerKey = Constants.ApiTestIssuerKey,
                        Enabled = true,
                        Immutable = false,
                    }));
            result.Should().BeAssignableTo<tbl_Issuers>();

            TestData.DestroyAsync().Wait();
        }

        [Fact]
        public async Task Lib_IssuerRepo_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.IssuerRepo.DeleteAsync(Guid.NewGuid());
            });
        }

        [Fact]
        public async Task Lib_IssuerRepo_DeleteV1_Success()
        {
            TestData.CreateAsync().Wait();

            var issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();

            var result = await UoW.IssuerRepo.DeleteAsync(issuer.Id);
            result.Should().BeTrue();

            TestData.DestroyAsync().Wait();
        }

        [Fact]
        public async Task Lib_IssuerRepo_GetV1_Success()
        {
            TestData.CreateAsync().Wait();

            var result = await UoW.IssuerRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Issuers>>();

            TestData.DestroyAsync().Wait();
        }

        [Fact]
        public async Task Lib_IssuerRepo_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.IssuerRepo.UpdateAsync(new tbl_Issuers());
            });
        }

        [Fact]
        public async Task Lib_IssuerRepo_UpdateV1_Success()
        {
            TestData.CreateAsync().Wait();

            var issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            issuer.Name += "(Updated)";

            var result = await UoW.IssuerRepo.UpdateAsync(issuer);
            result.Should().BeAssignableTo<tbl_Issuers>();

            TestData.DestroyAsync().Wait();
        }
    }
}
