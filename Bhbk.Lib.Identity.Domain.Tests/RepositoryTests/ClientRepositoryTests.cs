using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Domain.Tests.Primitives;
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

namespace Bhbk.Lib.Identity.Domain.Tests.RepositoryTests
{
    [Collection("LibraryTestsCollection")]
    public class ClientRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async Task Lib_ClientRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.ClientRepo.CreateAsync(
                    Mapper.Map<tbl_Clients>(new ClientCreate()));
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.ClientRepo.CreateAsync(
                    Mapper.Map<tbl_Clients>(new ClientCreate()
                        {
                            IssuerId = Guid.NewGuid(),
                            Name = Constants.ApiTestClient,
                            ClientKey = Constants.ApiTestClientKey,
                            ClientType = ClientType.user_agent.ToString(),
                            Enabled = true,
                            Immutable = false,
                        }));
            });
        }

        [Fact]
        public async Task Lib_ClientRepo_CreateV1_Success()
        {
            TestData.DestroyAsync().Wait();
            TestData.CreateAsync().Wait();

            var issuer = (await UoW.IssuerRepo.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();

            var result = await UoW.ClientRepo.CreateAsync(
                Mapper.Map<tbl_Clients>(new ClientCreate()
                    {
                        IssuerId = issuer.Id,
                        Name = Constants.ApiTestClient,
                        ClientKey = Constants.ApiTestClientKey,
                        ClientType = ClientType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    }));
            result.Should().BeAssignableTo<tbl_Clients>();
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
            TestData.DestroyAsync().Wait();
            TestData.CreateAsync().Wait();

            var client = (await UoW.ClientRepo.GetAsync(x => x.Name == Constants.ApiTestClient)).Single();

            var result = await UoW.ClientRepo.DeleteAsync(client.Id);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Lib_ClientRepo_GetV1_Success()
        {
            TestData.DestroyAsync().Wait();
            TestData.CreateAsync().Wait();

            var result = await UoW.ClientRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Clients>>();
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
            TestData.DestroyAsync().Wait();
            TestData.CreateAsync().Wait();

            var client = (await UoW.ClientRepo.GetAsync(x => x.Name == Constants.ApiTestClient)).Single();
            client.Name += "(Updated)";

            var result = await UoW.ClientRepo.UpdateAsync(client);
            result.Should().BeAssignableTo<tbl_Clients>();
        }
    }
}
