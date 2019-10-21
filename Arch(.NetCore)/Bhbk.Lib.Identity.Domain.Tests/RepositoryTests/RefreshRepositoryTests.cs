using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Domain.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class RefreshRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public void Repo_Refreshes_CreateV1_Fail()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                UoW.Refreshes.Create(
                    Mapper.Map<tbl_Refreshes>(new RefreshCreate()));
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Refreshes_CreateV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var issuer = UoW.Issuers.Get(x => x.Name == Constants.ApiTestIssuer).Single();
            var client = UoW.Clients.Get(x => x.Name == Constants.ApiTestClient).Single();
            var user = UoW.Users.Get(x => x.Email == Constants.ApiTestUser).Single();

            var result = UoW.Refreshes.Create(
                Mapper.Map<tbl_Refreshes>(new RefreshCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = Base64.CreateString(8),
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));
            result.Should().BeAssignableTo<tbl_Refreshes>();

            UoW.Commit();
        }

        [Fact]
        public void Repo_Refreshes_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Refreshes.Delete(new tbl_Refreshes());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Refreshes_DeleteV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var refresh = UoW.Refreshes.Get().First();

            UoW.Refreshes.Delete(refresh);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Refreshes_GetV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var results = UoW.Refreshes.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Refreshes>>();
        }

        [Fact]
        public void Repo_Refreshes_UpdateV1_Fail()
        {
            Assert.Throws<NotImplementedException>(() =>
            {
                UoW.Refreshes.Update(new tbl_Refreshes());
                UoW.Commit();
            });
        }
    }
}
