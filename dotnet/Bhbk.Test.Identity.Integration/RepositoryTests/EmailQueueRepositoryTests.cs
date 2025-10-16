using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Test.Identity.Integration.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class EmailQueueRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_EmailQueue_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateException>(() =>
            {
                UoW.Quotes.Post(new tbl_Quote());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_EmailQueue_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateEmails();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.UserName == TestData.User.UserName).ToLambda())
                .First();

            var result = UoW.EmailQueue.Post(
                Mapper.Map<tbl_EmailQueue>(new EmailV1()
                {
                    FromEmail = user.EmailAddress,
                    ToEmail = user.EmailAddress,
                    Subject = "Subject-" + AlphaNumeric.CreateString(4),
                    Body = "Body" + AlphaNumeric.CreateString(4),
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_EmailQueue>();
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
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateEmails();

            var email = UoW.EmailQueue.Get(QueryExpressionFactory.GetQueryExpression<tbl_EmailQueue>().ToLambda())
                .First();

            UoW.EmailQueue.Delete(email);
            UoW.Commit();
        }

        [Fact]
        public void Repo_EmailQueue_GetV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateEmails();

            var results = UoW.EmailQueue.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_EmailQueue>>();
            results.Count().Should().Be(UoW.EmailQueue.Count());
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
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateEmails();

            var email = UoW.EmailQueue.Get(QueryExpressionFactory.GetQueryExpression<tbl_EmailQueue>().ToLambda())
                .First();
            email.Subject += "(Updated)";

            var result = UoW.EmailQueue.Put(email);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_EmailQueue>();
        }
    }
}
