using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
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
    public class ClientRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async ValueTask Repo_Clients_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.Clients.CreateAsync(
                    Mapper.Map<tbl_Clients>(new ClientCreate()));

                await UoW.CommitAsync();
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.Clients.CreateAsync(
                    Mapper.Map<tbl_Clients>(new ClientCreate()
                        {
                            IssuerId = Guid.NewGuid(),
                            Name = Constants.ApiTestClient,
                            ClientKey = Constants.ApiTestClientKey,
                            ClientType = ClientType.user_agent.ToString(),
                            Enabled = true,
                            Immutable = false,
                        }));

                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Clients_CreateV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var issuer = (await UoW.Issuers.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();

            var result = await UoW.Clients.CreateAsync(
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

            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Clients_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            {
                await UoW.Clients.DeleteAsync(new tbl_Clients());
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Clients_DeleteV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var client = (await UoW.Clients.GetAsync(x => x.Name == Constants.ApiTestClient)).Single();

            await UoW.Clients.DeleteAsync(client);
            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Clients_GetV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var result = await UoW.Clients.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Clients>>();

            await UoW.CommitAsync();
        }

        [Fact]
        public async ValueTask Repo_Clients_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.Clients.UpdateAsync(new tbl_Clients());
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async ValueTask Repo_Clients_UpdateV1_Success()
        {
            await new TestData(UoW, Mapper).DestroyAsync();
            await new TestData(UoW, Mapper).CreateAsync();

            var client = (await UoW.Clients.GetAsync(x => x.Name == Constants.ApiTestClient)).Single();
            client.Name += "(Updated)";

            var result = await UoW.Clients.UpdateAsync(client);
            result.Should().BeAssignableTo<tbl_Clients>();

            await UoW.CommitAsync();
        }
    }
}
