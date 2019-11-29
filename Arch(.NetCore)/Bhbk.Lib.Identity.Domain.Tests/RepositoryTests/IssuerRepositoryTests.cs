﻿using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Domain.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class IssuerRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public void Repo_Issuers_CreateV1_Fail()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                UoW.Issuers.Create(
                    Mapper.Map<tbl_Issuers>(new IssuerCreate()));

                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Issuers_CreateV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var result = UoW.Issuers.Create(
                Mapper.Map<tbl_Issuers>(new IssuerCreate()
                    {
                        Name = Constants.ApiTestIssuer,
                        IssuerKey = Constants.ApiTestIssuerKey,
                        Enabled = true,
                        Immutable = false,
                    }));
            result.Should().BeAssignableTo<tbl_Issuers>();

            UoW.Commit();
        }

        [Fact]
        public void Repo_Issuers_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Issuers.Delete(new tbl_Issuers());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Issuers_DeleteV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var issuer = UoW.Issuers.Get(x => x.Name == Constants.ApiTestIssuer).Single();

            UoW.Issuers.Delete(issuer);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Issuers_GetV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var results = UoW.Issuers.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Issuers>>();
        }

        [Fact]
        public void Repo_Issuers_UpdateV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Issuers.Update(new tbl_Issuers());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Issuers_UpdateV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var issuer = UoW.Issuers.Get(x => x.Name == Constants.ApiTestIssuer).Single();
            issuer.Name += "(Updated)";

            var result = UoW.Issuers.Update(issuer);
            result.Should().BeAssignableTo<tbl_Issuers>();

            UoW.Commit();
        }
    }
}