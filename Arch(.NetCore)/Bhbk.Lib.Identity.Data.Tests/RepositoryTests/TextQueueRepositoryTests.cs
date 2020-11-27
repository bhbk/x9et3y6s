using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Models;
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

namespace Bhbk.Lib.Identity.Data.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class TextQueueRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_TextQueue_CreateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.TextQueue.Create(new uvw_TextQueue());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_TextQueue_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateTexts();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_User>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda())
                .First();

            var result = UoW.TextQueue.Create(
                Mapper.Map<uvw_TextQueue>(new TextV1()
                {
                    FromPhoneNumber = Constants.TestUserPhoneNumber,
                    ToPhoneNumber = Constants.TestUserPhoneNumber,
                    Body = "Body-" + Base64.CreateString(32),
                    SendAtUtc = DateTime.UtcNow,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<uvw_TextQueue>();
        }

        [Fact]
        public void Repo_TextQueue_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.TextQueue.Delete(new uvw_TextQueue());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_TextQueue_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateTexts();

            var text = UoW.TextQueue.Get(QueryExpressionFactory.GetQueryExpression<uvw_TextQueue>().ToLambda())
                .First();

            UoW.TextQueue.Delete(text);
            UoW.Commit();
        }

        [Fact]
        public void Repo_TextQueue_GetV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateTexts();

            var results = UoW.TextQueue.Get();
            results.Should().BeAssignableTo<IEnumerable<uvw_TextQueue>>();
            results.Count().Should().Be(UoW.TextQueue.Count());
        }

        [Fact]
        public void Repo_TextQueue_UpdateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.TextQueue.Update(new uvw_TextQueue());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_TextQueue_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateTexts();

            var text = UoW.TextQueue.Get(QueryExpressionFactory.GetQueryExpression<uvw_TextQueue>().ToLambda())
                .First();
            text.Body += "(Updated)";

            var result = UoW.TextQueue.Update(text);
            UoW.Commit();

            result.Should().BeAssignableTo<uvw_TextQueue>();
        }
    }
}
