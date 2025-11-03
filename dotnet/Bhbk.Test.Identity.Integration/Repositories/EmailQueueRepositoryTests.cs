using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Models.Alert;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Test.Identity.Integration.Repositories
{
    [Collection("RepositoryTests")]
    public class EmailQueueRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_EmailQueue_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateException>(() =>
            {
                UoW.EmailQueue.Post(new tbl_EmailQueue());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_EmailQueue_CreateV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithUser(u => u.WithDefaults())
                .Seed();

            var user = seed.Users.Values.First();

            var result = UoW.EmailQueue.Post(
                Mapper.Map<tbl_EmailQueue>(new EmailV1()
                {
                    FromEmail = user.EmailAddress,
                    ToEmail = user.EmailAddress,
                    Subject = "Subject-" + AlphaNumeric.CreateString(4),
                    Body = "Body-" + AlphaNumeric.CreateString(4),
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_EmailQueue>();

            UoW.EmailQueue.Delete(result);
            UoW.Commit();
        }

        [Fact]
        public void Repo_EmailQueue_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.EmailQueue.Delete(new tbl_EmailQueue());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_EmailQueue_DeleteV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithUser(u => u.WithDefaults())
                .Seed();

            var user = seed.Users.Values.First();

            var email = UoW.EmailQueue.Post(
                Mapper.Map<tbl_EmailQueue>(new EmailV1()
                {
                    FromEmail = user.EmailAddress,
                    ToEmail = user.EmailAddress,
                    Subject = "Subject-" + AlphaNumeric.CreateString(4),
                    Body = "Body-" + AlphaNumeric.CreateString(4),
                }));
            UoW.Commit();

            UoW.EmailQueue.Delete(email);
            UoW.Commit();
        }

        [Fact]
        public void Repo_EmailQueue_GetV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithUser(u => u.WithDefaults())
                .Seed();

            var user = seed.Users.Values.First();

            var email = UoW.EmailQueue.Post(
                Mapper.Map<tbl_EmailQueue>(new EmailV1()
                {
                    FromEmail = user.EmailAddress,
                    ToEmail = user.EmailAddress,
                    Subject = "Subject-" + AlphaNumeric.CreateString(4),
                    Body = "Body-" + AlphaNumeric.CreateString(4),
                }));
            UoW.Commit();

            var results = UoW.EmailQueue.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_EmailQueue>>();
            results.Count().Should().Be(UoW.EmailQueue.Count());

            UoW.EmailQueue.Delete(results);
            UoW.Commit();
        }

        [Fact]
        public void Repo_EmailQueue_UpdateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.EmailQueue.Put(new tbl_EmailQueue());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_EmailQueue_UpdateV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithUser(u => u.WithDefaults())
                .Seed();

            var user = seed.Users.Values.First();

            var email = UoW.EmailQueue.Post(
                Mapper.Map<tbl_EmailQueue>(new EmailV1()
                {
                    FromEmail = user.EmailAddress,
                    ToEmail = user.EmailAddress,
                    Subject = "Subject-" + AlphaNumeric.CreateString(4),
                    Body = "Body-" + AlphaNumeric.CreateString(4),
                }));
            UoW.Commit();

            email.Subject += "(Updated)";

            var result = UoW.EmailQueue.Put(email);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_EmailQueue>();

            UoW.EmailQueue.Delete(result);
            UoW.Commit();
        }
    }
}
