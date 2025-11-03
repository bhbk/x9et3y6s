using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Domain.Builders;
using Bhbk.Lib.Identity.Models.Me;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Test.Identity.Integration.Repositories
{
    [Collection("RepositoryTests")]
    public class QuoteRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Quotes_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateException>(() =>
            {
                UoW.Quotes.Post(new tbl_Quote());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Quotes_CreateV1_Success()
        {
            var builder = new QuoteBuilder().WithDefaults();

            var result = UoW.Quotes.Post(builder.Build());
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Quote>();

            UoW.Quotes.Delete(result);
            UoW.Commit();
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
            var builder = new QuoteBuilder().WithDefaults();
            var quote = UoW.Quotes.Post(builder.Build());
            UoW.Commit();

            UoW.Quotes.Delete(quote);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Quotes_GetV1_Success()
        {
            var builder = new QuoteBuilder().WithDefaults();
            var quote = UoW.Quotes.Post(builder.Build());
            UoW.Commit();

            var results = UoW.Quotes.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Quote>>();
            results.Count().Should().Be(UoW.Quotes.Count());

            UoW.Quotes.Delete(results);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Quotes_UpdateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Quotes.Put(new tbl_Quote());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Quotes_UpdateV1_Success()
        {
            var builder = new QuoteBuilder().WithDefaults();
            var quote = UoW.Quotes.Post(builder.Build());
            UoW.Commit();

            quote.Quote += "(Updated)";

            var result = UoW.Quotes.Put(quote);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Quote>();

            UoW.Quotes.Delete(result);
            UoW.Commit();
        }
    }
}
