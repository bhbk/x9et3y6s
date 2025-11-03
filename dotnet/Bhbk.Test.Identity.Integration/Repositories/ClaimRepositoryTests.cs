using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Domain.Builders;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Models.Admin;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Test.Identity.Integration.Repositories
{
    [Collection("RepositoryTests")]
    public class ClaimRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Claims_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateException>(() =>
            {
                UoW.Claims.Post(new tbl_Claim());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Claims_CreateV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .Seed();

            var result = UoW.Claims.Post(
                Mapper.Map<tbl_Claim>(new ClaimV1()
                {
                    IssuerId = seed.Issuer.Id,
                    Subject = "TestSubject",
                    Type = new ClaimBuilder().WithDefaults().GetClaimType(),
                    Value = AlphaNumeric.CreateString(8),
                    ValueType = "String",
                    IsDeletable = true,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Claim>();

            UoW.Claims.Delete(result);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Claims_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Claims.Delete(new tbl_Claim());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Claims_DeleteV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .WithClaim(c => c.WithDefaults())
                .Seed();

            var claim = seed.Claims.Values.First();

            UoW.Claims.Delete(claim);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Claims_GetV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .WithClaim(c => c.WithDefaults())
                .Seed();

            var results = UoW.Claims.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Claim>>();
            results.Count().Should().Be(UoW.Claims.Count());
        }

        [Fact]
        public void Repo_Claims_UpdateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Claims.Put(new tbl_Claim());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Claims_UpdateV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .WithClaim(c => c.WithDefaults())
                .Seed();

            var claim = seed.Claims.Values.First();
            claim.Value += "(Updated)";

            var result = UoW.Claims.Put(claim);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Claim>();
        }
    }
}
