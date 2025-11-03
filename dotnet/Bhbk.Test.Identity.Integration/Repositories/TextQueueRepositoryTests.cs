using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Models.Alert;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Test.Identity.Integration.Repositories
{
    [Collection("RepositoryTests")]
    public class TextQueueRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_TextQueue_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateException>(() =>
            {
                UoW.TextQueue.Post(new tbl_TextQueue());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_TextQueue_CreateV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithUser(u => u.WithDefaults())
                .Seed();

            var user = seed.Users.Values.First();

            var result = UoW.TextQueue.Post(
                Mapper.Map<tbl_TextQueue>(new TextV1()
                {
                    FromPhoneNumber = user.PhoneNumber,
                    ToPhoneNumber = user.PhoneNumber,
                    Body = "Body-" + Base64.CreateString(32),
                    SendAt = DateTime.UtcNow,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_TextQueue>();

            UoW.TextQueue.Delete(result);
            UoW.Commit();
        }

        [Fact]
        public void Repo_TextQueue_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.TextQueue.Delete(new tbl_TextQueue());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_TextQueue_DeleteV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithUser(u => u.WithDefaults())
                .Seed();

            var user = seed.Users.Values.First();

            var text = UoW.TextQueue.Post(
                Mapper.Map<tbl_TextQueue>(new TextV1()
                {
                    FromPhoneNumber = user.PhoneNumber,
                    ToPhoneNumber = user.PhoneNumber,
                    Body = "Body-" + Base64.CreateString(32),
                    SendAt = DateTime.UtcNow,
                }));
            UoW.Commit();

            UoW.TextQueue.Delete(text);
            UoW.Commit();
        }

        [Fact]
        public void Repo_TextQueue_GetV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithUser(u => u.WithDefaults())
                .Seed();

            var user = seed.Users.Values.First();

            var text = UoW.TextQueue.Post(
                Mapper.Map<tbl_TextQueue>(new TextV1()
                {
                    FromPhoneNumber = user.PhoneNumber,
                    ToPhoneNumber = user.PhoneNumber,
                    Body = "Body-" + Base64.CreateString(32),
                    SendAt = DateTime.UtcNow,
                }));
            UoW.Commit();

            var results = UoW.TextQueue.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_TextQueue>>();
            results.Count().Should().Be(UoW.TextQueue.Count());

            UoW.TextQueue.Delete(results);
            UoW.Commit();
        }

        [Fact]
        public void Repo_TextQueue_UpdateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.TextQueue.Put(new tbl_TextQueue());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_TextQueue_UpdateV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithUser(u => u.WithDefaults())
                .Seed();

            var user = seed.Users.Values.First();

            var text = UoW.TextQueue.Post(
                Mapper.Map<tbl_TextQueue>(new TextV1()
                {
                    FromPhoneNumber = user.PhoneNumber,
                    ToPhoneNumber = user.PhoneNumber,
                    Body = "Body-" + Base64.CreateString(32),
                    SendAt = DateTime.UtcNow,
                }));
            UoW.Commit();

            text.Body += "(Updated)";

            var result = UoW.TextQueue.Put(text);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_TextQueue>();

            UoW.TextQueue.Delete(result);
            UoW.Commit();
        }
    }
}
