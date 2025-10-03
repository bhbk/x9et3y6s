using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EF.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class QuoteRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Quotes_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateException>(() =>
            {
                UoW.Quotes.Create(new tbl_Quote());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Quotes_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateQuotes();

            var result = UoW.Quotes.Create(
                Mapper.Map<tbl_Quote>(new QuoteV1
                {
                    author = TestData.Quote.Author,
                    quote = "Quote-" + Base64.CreateString(4),
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Quote>();
        }

        [Fact]
        public void Repo_Quotes_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Quotes.Delete(new tbl_Quote());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Quotes_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateQuotes();

            var quote = UoW.Quotes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Quote>()
                .Where(x => x.Author == TestData.Quote.Author).ToLambda())
                .First();

            UoW.Quotes.Delete(quote);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Quotes_GetV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateQuotes();

            var results = UoW.Quotes.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Quote>>();
            results.Count().Should().Be(UoW.Quotes.Count());
        }

        [Fact]
        public void Repo_Quotes_UpdateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Quotes.Update(new tbl_Quote());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Quotes_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW, TestData);
            data.Destroy();
            data.CreateQuotes();

            var quote = UoW.Quotes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Quote>()
                .Where(x => x.Author == TestData.Quote.Author).ToLambda())
                .First();
            quote.Quote += "(Updated)";

            var result = UoW.Quotes.Update(quote);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Quote>();
        }
    }
}
