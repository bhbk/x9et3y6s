using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Test.Identity.Integration.Repositories
{
    [Collection("RepositoryTests")]
    public class RefreshRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Refreshes_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateException>(() =>
            {
                UoW.Refreshes.Post(new tbl_Refresh());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Refreshes_CreateV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults())
                .WithUser(u => u.WithDefaults())
                .Seed();

            var result = UoW.Refreshes.Post(
                Mapper.Map<tbl_Refresh>(new RefreshV1()
                {
                    IssuerId = seed.Issuer.Id,
                    AudienceId = seed.Audiences.Values.First().Id,
                    UserId = seed.Users.Values.First().Id,
                    RefreshType = ConsumerType.User.ToString(),
                    RefreshValue = Base64.CreateString(8),
                    ValidFrom = DateTime.UtcNow,
                    ValidTo = DateTime.UtcNow.AddSeconds(60),
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Refresh>();

            UoW.Refreshes.Delete(result);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Refreshes_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Refreshes.Delete(new tbl_Refresh());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Refreshes_DeleteV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults())
                .WithUser(u => u.WithDefaults())
                .Seed();

            var refresh = UoW.Refreshes.Post(
                Mapper.Map<tbl_Refresh>(new RefreshV1()
                {
                    IssuerId = seed.Issuer.Id,
                    AudienceId = seed.Audiences.Values.First().Id,
                    UserId = seed.Users.Values.First().Id,
                    RefreshType = ConsumerType.User.ToString(),
                    RefreshValue = Base64.CreateString(8),
                    ValidFrom = DateTime.UtcNow,
                    ValidTo = DateTime.UtcNow.AddSeconds(60),
                }));
            UoW.Commit();

            UoW.Refreshes.Delete(refresh);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Refreshes_GetV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults())
                .WithUser(u => u.WithDefaults())
                .Seed();

            UoW.Refreshes.Post(
                Mapper.Map<tbl_Refresh>(new RefreshV1()
                {
                    IssuerId = seed.Issuer.Id,
                    AudienceId = seed.Audiences.Values.First().Id,
                    UserId = seed.Users.Values.First().Id,
                    RefreshType = ConsumerType.User.ToString(),
                    RefreshValue = Base64.CreateString(8),
                    ValidFrom = DateTime.UtcNow,
                    ValidTo = DateTime.UtcNow.AddSeconds(60),
                }));
            UoW.Commit();

            var results = UoW.Refreshes.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Refresh>>();
            results.Count().Should().Be(UoW.Refreshes.Count());

            UoW.Refreshes.Delete(results);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Refreshes_UpdateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Refreshes.Put(new tbl_Refresh());
                UoW.Commit();
            });
        }
    }
}
