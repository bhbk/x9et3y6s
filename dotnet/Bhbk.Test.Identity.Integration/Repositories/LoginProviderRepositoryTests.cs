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
    public class LoginProviderRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_LoginProviders_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateException>(() =>
            {
                UoW.LoginProviders.Post(new tbl_LoginProvider());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_LoginProviders_CreateV1_Success()
        {
            var builder = new LoginProviderBuilder().WithDefaults();

            var result = UoW.LoginProviders.Post(builder.Build());
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_LoginProvider>();

            UoW.LoginProviders.Delete(result);
            UoW.Commit();
        }

        [Fact]
        public void Repo_LoginProviders_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.LoginProviders.Delete(new tbl_LoginProvider());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_LoginProviders_DeleteV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithLoginProvider(lp => lp.WithDefaults())
                .Seed();

            var loginProvider = seed.LoginProviders.Values.First();

            UoW.LoginProviders.Delete(loginProvider);
            UoW.Commit();
        }

        [Fact]
        public void Repo_LoginProviders_GetV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithLoginProvider(lp => lp.WithDefaults())
                .Seed();

            var results = UoW.LoginProviders.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_LoginProvider>>();
            results.Count().Should().Be(UoW.LoginProviders.Count());
        }

        [Fact]
        public void Repo_LoginProviders_UpdateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.LoginProviders.Put(new tbl_LoginProvider());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_LoginProviders_UpdateV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithLoginProvider(lp => lp.WithDefaults())
                .Seed();

            var loginProvider = seed.LoginProviders.Values.First();
            loginProvider.Name += "(Updated)";

            var result = UoW.LoginProviders.Put(loginProvider);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_LoginProvider>();
        }
    }
}
