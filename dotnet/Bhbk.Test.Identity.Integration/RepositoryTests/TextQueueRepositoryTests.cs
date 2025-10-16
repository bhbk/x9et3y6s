using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Test.Identity.Integration.RepositoryTests
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
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateTexts();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.UserName == TestData.User.UserName).ToLambda())
                .First();

            var result = UoW.TextQueue.Post(
                Mapper.Map<tbl_TextQueue>(new TextV1()
                {
                    FromPhoneNumber = TestData.User.PhoneNumber,
                    ToPhoneNumber = TestData.User.PhoneNumber,
                    Body = "Body-" + Base64.CreateString(32),
                    SendAtUtc = DateTime.UtcNow,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_TextQueue>();
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
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateTexts();

            var text = UoW.TextQueue.Get(QueryExpressionFactory.GetQueryExpression<tbl_TextQueue>().ToLambda())
                .First();

            UoW.TextQueue.Delete(text);
            UoW.Commit();
        }

        [Fact]
        public void Repo_TextQueue_GetV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateTexts();

            var results = UoW.TextQueue.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_TextQueue>>();
            results.Count().Should().Be(UoW.TextQueue.Count());
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
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateTexts();

            var text = UoW.TextQueue.Get(QueryExpressionFactory.GetQueryExpression<tbl_TextQueue>().ToLambda())
                .First();
            text.Body += "(Updated)";

            var result = UoW.TextQueue.Put(text);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_TextQueue>();
        }
    }
}
