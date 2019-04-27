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
    public class IssuerRepositoryTests
    {
        private StartupTests _factory;

        public IssuerRepositoryTests(StartupTests factory) => _factory = factory;

        [Fact(Skip = "NotImplemented")]
        public async Task Lib_IssuerRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await _factory.UoW.IssuerRepo.CreateAsync(new IssuerCreate());
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await _factory.UoW.IssuerRepo.CreateAsync(
                    new IssuerCreate()
                    {
                        Name = Strings.ApiUnitTestIssuer,
                        IssuerKey = Strings.ApiUnitTestIssuerKey,
                        Enabled = true,
                        Immutable = false,
                    });
            });
        }

        [Fact]
        public async Task Lib_IssuerRepo_CreateV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var result = await _factory.UoW.IssuerRepo.CreateAsync(
                new IssuerCreate()
                {
                    Name = Strings.ApiUnitTestIssuer,
                    IssuerKey = Strings.ApiUnitTestIssuerKey,
                    Enabled = true,
                    Immutable = false,
                });
            result.Should().BeAssignableTo<tbl_Issuers>();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_IssuerRepo_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _factory.UoW.IssuerRepo.DeleteAsync(Guid.NewGuid());
            });
        }

        [Fact]
        public async Task Lib_IssuerRepo_DeleteV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).First();

            var result = await _factory.UoW.IssuerRepo.DeleteAsync(issuer.Id);
            result.Should().BeTrue();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_IssuerRepo_GetV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var result = await _factory.UoW.IssuerRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Issuers>>();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_IssuerRepo_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _factory.UoW.IssuerRepo.UpdateAsync(new tbl_Issuers());
            });
        }

        [Fact]
        public async Task Lib_IssuerRepo_UpdateV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).First();
            issuer.Name += "(Updated)";

            var result = await _factory.UoW.IssuerRepo.UpdateAsync(issuer);
            result.Should().BeAssignableTo<tbl_Issuers>();

            await _factory.TestData.DestroyAsync();
        }
    }
}
