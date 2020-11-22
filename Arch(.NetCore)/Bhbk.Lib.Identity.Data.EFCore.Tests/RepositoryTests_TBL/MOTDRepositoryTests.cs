using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EFCore.Models_TBL;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests_TBL
{
    [Collection("RepositoryTests_TBL")]
    public class MOTDRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public void Repo_MOTDs_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.MOTDs.Create(new tbl_MOTD());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_MOTDs_CreateV1_Success()
        {
            var data = new TestDataFactory_TBL(UoW);
            data.Destroy();
            data.CreateMOTDs();

            var result = UoW.MOTDs.Create(
                Mapper.Map<tbl_MOTD>(new MOTDTssV1
                {
                    author = Constants.TestMotdAuthor,
                    quote = "Quote-" + Base64.CreateString(4),
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_MOTD>();
        }

        [Fact]
        public void Repo_MOTDs_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.MOTDs.Delete(new tbl_MOTD());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_MOTDs_DeleteV1_Success()
        {
            var data = new TestDataFactory_TBL(UoW);
            data.Destroy();
            data.CreateMOTDs();

            var MOTD = UoW.MOTDs.Get(QueryExpressionFactory.GetQueryExpression<tbl_MOTD>()
                .Where(x => x.Author == Constants.TestMotdAuthor).ToLambda())
                .First();

            UoW.MOTDs.Delete(MOTD);
            UoW.Commit();
        }

        [Fact]
        public void Repo_MOTDs_GetV1_Success()
        {
            var data = new TestDataFactory_TBL(UoW);
            data.Destroy();
            data.CreateMOTDs();

            var results = UoW.MOTDs.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_MOTD>>();
            results.Count().Should().Be(UoW.MOTDs.Count());
        }

        [Fact]
        public void Repo_MOTDs_UpdateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.MOTDs.Update(new tbl_MOTD());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_MOTDs_UpdateV1_Success()
        {
            var data = new TestDataFactory_TBL(UoW);
            data.Destroy();
            data.CreateMOTDs();

            var MOTD = UoW.MOTDs.Get(QueryExpressionFactory.GetQueryExpression<tbl_MOTD>()
                .Where(x => x.Author == Constants.TestMotdAuthor).ToLambda())
                .First();
            MOTD.Quote += "(Updated)";

            var result = UoW.MOTDs.Update(MOTD);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_MOTD>();
        }
    }
}
