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
    public class ClientRepositoryTests
    {
        private StartupTests _factory;

        public ClientRepositoryTests(StartupTests factory) => _factory = factory;

        [Fact(Skip = "NotImplemented")]
        public async Task Lib_ClientRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await _factory.UoW.ClientRepo.CreateAsync(new ClientCreate());
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await _factory.UoW.ClientRepo.CreateAsync(
                    new ClientCreate()
                    {
                        IssuerId = Guid.NewGuid(),
                        Name = Strings.ApiUnitTestClient,
                        ClientKey = Strings.ApiUnitTestClientKey,
                        ClientType = ClientType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    });
            });
        }

        [Fact]
        public async Task Lib_ClientRepo_CreateV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var issuer = (await _factory.UoW.IssuerRepo.GetAsync(x => x.Name == Strings.ApiUnitTestIssuer)).Single();

            var result = await _factory.UoW.ClientRepo.CreateAsync(
                new ClientCreate()
                {
                    IssuerId = issuer.Id,
                    Name = Strings.ApiUnitTestClient,
                    ClientKey = Strings.ApiUnitTestClientKey,
                    ClientType = ClientType.user_agent.ToString(),
                    Enabled = true,
                    Immutable = false,
                });
            result.Should().BeAssignableTo<tbl_Clients>();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_ClientRepo_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _factory.UoW.ClientRepo.DeleteAsync(Guid.NewGuid());
            });
        }

        [Fact]
        public async Task Lib_ClientRepo_DeleteV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).First();

            var result = await _factory.UoW.ClientRepo.DeleteAsync(client.Id);
            result.Should().BeTrue();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_ClientRepo_GetV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var result = await _factory.UoW.ClientRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Clients>>();

            await _factory.TestData.DestroyAsync();
        }

        [Fact]
        public async Task Lib_ClientRepo_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _factory.UoW.ClientRepo.UpdateAsync(new tbl_Clients());
            });
        }

        [Fact]
        public async Task Lib_ClientRepo_UpdateV1_Success()
        {
            await _factory.TestData.CreateAsync();

            var client = (await _factory.UoW.ClientRepo.GetAsync(x => x.Name == Strings.ApiUnitTestClient)).First();
            client.Name += "(Updated)";

            var result = await _factory.UoW.ClientRepo.UpdateAsync(client);
            result.Should().BeAssignableTo<tbl_Clients>();

            await _factory.TestData.DestroyAsync();
        }
    }
}
