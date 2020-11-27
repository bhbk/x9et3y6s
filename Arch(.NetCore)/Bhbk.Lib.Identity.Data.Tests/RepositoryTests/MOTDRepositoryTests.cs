using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Models.Me;
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
    public class MOTDRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_MOTDs_CreateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.MOTDs.Create(new uvw_MOTD());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_MOTDs_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateMOTDs();

            var result = UoW.MOTDs.Create(
                Mapper.Map<uvw_MOTD>(new MOTDTssV1
                {
                    author = Constants.TestMotdAuthor,
                    quote = "Quote-" + Base64.CreateString(4),
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<uvw_MOTD>();
        }

        [Fact]
        public void Repo_MOTDs_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.MOTDs.Delete(new uvw_MOTD());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_MOTDs_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateMOTDs();

            var MOTD = UoW.MOTDs.Get(QueryExpressionFactory.GetQueryExpression<uvw_MOTD>()
                .Where(x => x.Author == Constants.TestMotdAuthor).ToLambda())
                .First();

            UoW.MOTDs.Delete(MOTD);
            UoW.Commit();
        }

        [Fact]
        public void Repo_MOTDs_GetV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateMOTDs();

            var results = UoW.MOTDs.Get();
            results.Should().BeAssignableTo<IEnumerable<uvw_MOTD>>();
            results.Count().Should().Be(UoW.MOTDs.Count());
        }

        [Fact]
        public void Repo_MOTDs_UpdateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.MOTDs.Update(new uvw_MOTD());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_MOTDs_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateMOTDs();

            var MOTD = UoW.MOTDs.Get(QueryExpressionFactory.GetQueryExpression<uvw_MOTD>()
                .Where(x => x.Author == Constants.TestMotdAuthor).ToLambda())
                .First();
            MOTD.Quote += "(Updated)";

            var result = UoW.MOTDs.Update(MOTD);
            UoW.Commit();

            result.Should().BeAssignableTo<uvw_MOTD>();
        }
    }
}
