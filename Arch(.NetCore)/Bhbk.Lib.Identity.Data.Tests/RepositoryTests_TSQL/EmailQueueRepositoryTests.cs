using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Models_TSQL;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.Tests.RepositoryTests_TSQL
{
    [Collection("RepositoryTests_TSQL")]
    public class EmailQueueRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_EmailQueue_CreateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.EmailQueue.Create(new uvw_EmailQueue());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_EmailQueue_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateEmails();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_User>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda())
                .First();

            var result = UoW.EmailQueue.Create(
                Mapper.Map<uvw_EmailQueue>(new EmailV1()
                {
                    FromEmail = user.EmailAddress,
                    ToEmail = user.EmailAddress,
                    Subject = "Subject-" + Base64.CreateString(4),
                    Body = "Body-" + Base64.CreateString(32),
                    SendAtUtc = DateTime.UtcNow,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<uvw_EmailQueue>();
        }

        [Fact]
        public void Repo_EmailQueue_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.EmailQueue.Delete(new uvw_EmailQueue());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_EmailQueue_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateEmails();

            var email = UoW.EmailQueue.Get(QueryExpressionFactory.GetQueryExpression<uvw_EmailQueue>().ToLambda())
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
            results.Should().BeAssignableTo<IEnumerable<uvw_EmailQueue>>();
            results.Count().Should().Be(UoW.EmailQueue.Count());
        }

        [Fact]
        public void Repo_EmailQueue_UpdateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.EmailQueue.Update(new uvw_EmailQueue());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_EmailQueue_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateEmails();

            var email = UoW.EmailQueue.Get(QueryExpressionFactory.GetQueryExpression<uvw_EmailQueue>().ToLambda())
                .First();
            email.Subject += "(Updated)";

            var result = UoW.EmailQueue.Update(email);
            UoW.Commit();

            result.Should().BeAssignableTo<uvw_EmailQueue>();
        }
    }
}
