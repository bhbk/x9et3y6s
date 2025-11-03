using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Domain.Builders;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Models.Admin;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Test.Identity.Integration.Repositories
{
    [Collection("RepositoryTests")]
    public class AudienceRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Audiences_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateException>(() =>
            {
                UoW.Audiences.Post(new tbl_Audience());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Audiences_CreateV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .Seed();

            var result = UoW.Audiences.Post(
                Mapper.Map<tbl_Audience>(new AudienceV1()
                {
                    IssuerId = seed.Issuer.Id,
                    Name = new AudienceBuilder().WithDefaults().GetName(),
                    IsLockedOut = false,
                    IsDeletable = true,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Audience>();

            UoW.Audiences.Delete(result);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Audiences_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Audiences.Delete(new tbl_Audience());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Audiences_DeleteV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults())
                .Seed();

            var audience = seed.Audiences.Values.First();

            UoW.Audiences.Delete(audience);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Audiences_GetV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults())
                .Seed();

            var results = UoW.Audiences.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Audience>>();
            results.Count().Should().Be(UoW.Audiences.Count());
        }

        [Fact]
        public void Repo_Audiences_UpdateV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Audiences.Put(new tbl_Audience());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Audiences_UpdateV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults())
                .Seed();

            var audience = seed.Audiences.Values.First();
            audience.Name += "(Updated)";

            UoW.Audiences.Put(audience);
            UoW.Commit();

            audience.Should().BeAssignableTo<tbl_Audience>();
        }
    }
}
