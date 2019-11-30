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
    public class AudienceRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public void Repo_Audiences_CreateV1_Fail()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                UoW.Audiences.Create(
                    Mapper.Map<tbl_Audiences>(new AudienceCreate()));

                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Audiences_CreateV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var issuer = UoW.Issuers.Get(x => x.Name == Constants.ApiTestIssuer).Single();

            var result = UoW.Audiences.Create(
                Mapper.Map<tbl_Audiences>(new AudienceCreate()
                    {
                        IssuerId = issuer.Id,
                        Name = Constants.ApiTestClient,
                        AudienceType = AudienceType.user_agent.ToString(),
                        Enabled = true,
                        Immutable = false,
                    }));
            result.Should().BeAssignableTo<tbl_Audiences>();

            UoW.Commit();
        }

        [Fact]
        public void Repo_Audiences_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Audiences.Delete(new tbl_Audiences());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Audiences_DeleteV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var client = UoW.Audiences.Get(x => x.Name == Constants.ApiTestClient).Single();

            UoW.Audiences.Delete(client);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Audiences_GetV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var results = UoW.Audiences.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Audiences>>();
        }

        [Fact]
        public void Repo_Audiences_UpdateV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Audiences.Update(new tbl_Audiences());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Audiences_UpdateV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var client = UoW.Audiences.Get(x => x.Name == Constants.ApiTestClient).Single();
            client.Name += "(Updated)";

            var result = UoW.Audiences.Update(client);
            result.Should().BeAssignableTo<tbl_Audiences>();

            UoW.Commit();
        }
    }
}
