using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests_DIRECT
{
    [Collection("RepositoryTests_DIRECT")]
    public class TextQueueRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public void Repo_TextQueue_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.MOTDs.Create(new tbl_MOTD());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_TextQueue_CreateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).CreateText(3);

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda())
                .First();

            var result = UoW.TextQueue.Create(
                Mapper.Map<tbl_TextQueue>(new TextV1()
                {
                    FromId = user.Id,
                    FromPhoneNumber = Constants.TestUserPhoneNumber,
                    ToId = user.Id,
                    ToPhoneNumber = Constants.TestUserPhoneNumber,
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
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).CreateText(3);

            var text = UoW.TextQueue.Get(QueryExpressionFactory.GetQueryExpression<tbl_TextQueue>().ToLambda())
                .First();

            UoW.TextQueue.Delete(text);
            UoW.Commit();
        }

        [Fact]
        public void Repo_TextQueue_GetV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).CreateText(3);

            var results = UoW.TextQueue.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_TextQueue>>();
            results.Count().Should().Be(UoW.TextQueue.Count());
        }

        [Fact]
        public void Repo_TextQueue_UpdateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.TextQueue.Update(new tbl_TextQueue());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_TextQueue_UpdateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).CreateText(3);

            var text = UoW.TextQueue.Get(QueryExpressionFactory.GetQueryExpression<tbl_TextQueue>().ToLambda())
                .First();
            text.Body += "(Updated)";

            var result = UoW.TextQueue.Update(text);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_TextQueue>();
        }
    }
}
