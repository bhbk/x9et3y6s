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
using Xunit;

namespace Bhbk.Lib.Identity.Domain.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class ClientRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public void Repo_Clients_CreateV1_Fail()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                UoW.Clients.Create(
                    Mapper.Map<tbl_Clients>(new ClientCreate()));

                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Clients_CreateV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var issuer = UoW.Issuers.Get(x => x.Name == Constants.ApiTestIssuer).Single();

            var result = UoW.Clients.Create(
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

            UoW.Commit();
        }

        [Fact]
        public void Repo_Clients_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Clients.Delete(new tbl_Clients());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Clients_DeleteV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var client = UoW.Clients.Get(x => x.Name == Constants.ApiTestClient).Single();

            UoW.Clients.Delete(client);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Clients_GetV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var results = UoW.Clients.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Clients>>();
        }

        [Fact]
        public void Repo_Clients_UpdateV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Clients.Update(new tbl_Clients());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Clients_UpdateV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var client = UoW.Clients.Get(x => x.Name == Constants.ApiTestClient).Single();
            client.Name += "(Updated)";

            var result = UoW.Clients.Update(client);
            result.Should().BeAssignableTo<tbl_Clients>();

            UoW.Commit();
        }
    }
}
