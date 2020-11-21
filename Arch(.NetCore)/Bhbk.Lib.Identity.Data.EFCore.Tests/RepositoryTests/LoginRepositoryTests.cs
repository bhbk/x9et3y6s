﻿using Bhbk.Lib.Identity.Data.EFCore.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class LoginRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Logins_CreateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.Logins.Create(new uvw_Login());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Logins_CreateV1_Success()
        {
            new TestDataFactory(UoW, Mapper).Destroy();
            new TestDataFactory(UoW, Mapper).Create();

            var result = UoW.Logins.Create(
                Mapper.Map<uvw_Login>(new LoginV1()
                {
                    Name = Constants.TestLogin,
                    LoginKey = Constants.TestLoginKey,
                    IsDeletable = false,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<uvw_Login>();
        }

        [Fact]
        public void Repo_Logins_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Logins.Delete(new uvw_Login());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Logins_DeleteV1_Success()
        {
            new TestDataFactory(UoW, Mapper).Destroy();
            new TestDataFactory(UoW, Mapper).Create();

            var login = UoW.Logins.Get(QueryExpressionFactory.GetQueryExpression<uvw_Login>()
                .Where(x => x.Name == Constants.TestLogin).ToLambda())
                .Single();

            UoW.Logins.Delete(login);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Logins_GetV1_Success()
        {
            new TestDataFactory(UoW, Mapper).Destroy();
            new TestDataFactory(UoW, Mapper).Create();

            var results = UoW.Logins.Get();
            results.Should().BeAssignableTo<IEnumerable<uvw_Login>>();
            results.Count().Should().Be(UoW.Logins.Count());
        }

        [Fact]
        public void Repo_Logins_UpdateV1_Fail()
        {
            Assert.Throws<SqlException>(() =>
            {
                UoW.Logins.Update(new uvw_Login());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Logins_UpdateV1_Success()
        {
            new TestDataFactory(UoW, Mapper).Destroy();
            new TestDataFactory(UoW, Mapper).Create();

            var login = UoW.Logins.Get(QueryExpressionFactory.GetQueryExpression<uvw_Login>()
                .Where(x => x.Name == Constants.TestLogin).ToLambda())
                .Single();
            login.Name += "(Updated)";

            var result = UoW.Logins.Update(login);
            UoW.Commit();

            result.Should().BeAssignableTo<uvw_Login>();
        }
    }
}
