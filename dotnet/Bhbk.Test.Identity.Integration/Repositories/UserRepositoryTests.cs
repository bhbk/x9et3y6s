using Bhbk.Lib.Cryptography.Entropy;
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
    public class UserRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Users_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateException>(() =>
            {
                UoW.Users.Post(new tbl_User());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Users_CreateV1_Success()
        {
            var builder = new UserBuilder().WithDefaults();

            var result = UoW.Users.Post(builder.Build(), builder.GetPassword());
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_User>();

            UoW.Users.Delete(result);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Users_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Users.Delete(new tbl_User());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Users_DeleteV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithUser(u => u.WithDefaults())
                .Seed();

            var user = seed.Users.Values.First();

            UoW.Users.Delete(user);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Users_GetV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithUser(u => u.WithDefaults())
                .Seed();

            var results = UoW.Users.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_User>>();
            results.Count().Should().Be(UoW.Users.Count());
        }

        [Fact]
        public void Repo_Users_UpdateV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Users.Put(new tbl_User());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Users_UpdateV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithUser(u => u.WithDefaults())
                .Seed();

            var user = seed.Users.Values.First();
            user.FirstName += "(Updated)";
            user.LastName += "(Updated)";

            UoW.Users.Put(user);
            UoW.Commit();

            user.Should().BeAssignableTo<tbl_User>();
        }
    }
}
