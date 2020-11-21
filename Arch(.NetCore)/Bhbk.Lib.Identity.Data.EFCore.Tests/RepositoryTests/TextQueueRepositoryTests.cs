using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class TextQueueRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public void Repo_TextQueue_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.TextQueue.Create(new uvw_TextQueue());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_TextQueue_CreateV1_Success()
        {
            new TestDataFactory(UoW, Mapper).Destroy();
            new TestDataFactory(UoW, Mapper).CreateText(3);

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<uvw_User>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda())
                .First();

            var result = UoW.TextQueue.Create(
                Mapper.Map<uvw_TextQueue>(new TextV1()
                {
                    FromId = user.Id,
                    FromPhoneNumber = Constants.TestUserPhoneNumber,
                    ToId = user.Id,
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
            new TestDataFactory(UoW, Mapper).Destroy();
            new TestDataFactory(UoW, Mapper).CreateText(3);

            var text = UoW.TextQueue.Get(QueryExpressionFactory.GetQueryExpression<uvw_TextQueue>().ToLambda())
                .First();

            UoW.TextQueue.Delete(text);
            UoW.Commit();
        }

        [Fact]
        public void Repo_TextQueue_GetV1_Success()
        {
            new TestDataFactory(UoW, Mapper).Destroy();
            new TestDataFactory(UoW, Mapper).CreateText(3);

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
            new TestDataFactory(UoW, Mapper).Destroy();
            new TestDataFactory(UoW, Mapper).CreateText(3);

            var text = UoW.TextQueue.Get(QueryExpressionFactory.GetQueryExpression<uvw_TextQueue>().ToLambda())
                .First();
            text.Body += "(Updated)";

            var result = UoW.TextQueue.Update(text);
            UoW.Commit();

            result.Should().BeAssignableTo<uvw_TextQueue>();
        }
    }
}
