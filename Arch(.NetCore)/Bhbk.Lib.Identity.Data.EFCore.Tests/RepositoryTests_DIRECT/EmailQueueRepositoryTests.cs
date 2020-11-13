using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests_DIRECT
{
    [Collection("RepositoryTests_DIRECT")]
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
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).CreateEmail(3);

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.UserName == Constants.TestUser).ToLambda())
                .First();

            var result = UoW.EmailQueue.Create(
                Mapper.Map<tbl_EmailQueue>(new EmailV1()
                {
                    FromId = user.Id,
                    FromEmail = user.EmailAddress,
                    ToId = user.Id,
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
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).CreateEmail(3);

            var email = UoW.EmailQueue.Get(QueryExpressionFactory.GetQueryExpression<tbl_EmailQueue>().ToLambda())
                .First();

            UoW.EmailQueue.Delete(email);
            UoW.Commit();
        }

        [Fact]
        public void Repo_EmailQueue_GetV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).CreateEmail(3);

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
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).CreateEmail(3);

            var email = UoW.EmailQueue.Get(QueryExpressionFactory.GetQueryExpression<tbl_EmailQueue>().ToLambda())
                .First();
            email.Subject += "(Updated)";

            var result = UoW.EmailQueue.Update(email);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_EmailQueue>();
        }
    }
}
