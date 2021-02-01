using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Models_Tbl;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Primitives.Tests.Constants;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.Tests.RepositoryTests_Tbl
{
    [Collection("RepositoryTests_Tbl")]
    public class EmailQueueRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public void Repo_EmailQueue_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.MOTDs.Create(new tbl_MOTD());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_EmailQueue_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateEmails();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.UserName == TestDefaultConstants.UserName).ToLambda())
                .First();

            var result = UoW.EmailQueue.Create(
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
            var data = new TestDataFactory(UoW);
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
            var data = new TestDataFactory(UoW);
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
                UoW.EmailQueue.Update(new tbl_EmailQueue());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_EmailQueue_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateEmails();

            var email = UoW.EmailQueue.Get(QueryExpressionFactory.GetQueryExpression<tbl_EmailQueue>().ToLambda())
                .First();
            email.Subject += "(Updated)";

            var result = UoW.EmailQueue.Update(email);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_EmailQueue>();
        }
    }
}
