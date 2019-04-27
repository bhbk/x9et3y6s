using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
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
    public class ClientRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async Task Lib_ClientRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.ClientRepo.CreateAsync(new ClientCreate());
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.ClientRepo.CreateAsync(
                    new ClientCreate()
                    {
                        IssuerId = Guid.NewGuid(),
                        Name = Constants.ApiUnitTestClient,
                        ClientKey = Constants.ApiUnitTestClientKey,
                        ClientType = ClientType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    });
            });
        }

        [Fact]
        public async Task Lib_ClientRepo_CreateV1_Success()
        {
            await TestData.CreateAsync();

            var issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == Constants.ApiUnitTestIssuer)).Single();

            var result = await UoW.ClientRepo.CreateAsync(
                new ClientCreate()
                {
                    IssuerId = issuer.Id,
                    Name = Constants.ApiUnitTestClient,
                    ClientKey = Constants.ApiUnitTestClientKey,
                    ClientType = ClientType.user_agent.ToString(),
                    Enabled = true,
                    Immutable = false,
                });
            result.Should().BeAssignableTo<tbl_Clients>();

            await TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_ClientRepo_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.ClientRepo.DeleteAsync(Guid.NewGuid());
            });
        }

        [Fact]
        public async Task Lib_ClientRepo_DeleteV1_Success()
        {
            await TestData.CreateAsync();

            var client = (await UoW.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).First();

            var result = await UoW.ClientRepo.DeleteAsync(client.Id);
            result.Should().BeTrue();

            await TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_ClientRepo_GetV1_Success()
        {
            await TestData.CreateAsync();

            var result = await UoW.ClientRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Clients>>();

            await TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_ClientRepo_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.ClientRepo.UpdateAsync(new tbl_Clients());
            });
        }

        [Fact]
        public async Task Lib_ClientRepo_UpdateV1_Success()
        {
            await TestData.CreateAsync();

            var client = (await UoW.ClientRepo.GetAsync(x => x.Name == Constants.ApiUnitTestClient)).First();
            client.Name += "(Updated)";

            var result = await UoW.ClientRepo.UpdateAsync(client);
            result.Should().BeAssignableTo<tbl_Clients>();

            await TestData.DestroyAsync();
        }
    }
}
