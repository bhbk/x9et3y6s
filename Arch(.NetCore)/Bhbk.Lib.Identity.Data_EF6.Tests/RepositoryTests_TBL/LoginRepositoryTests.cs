﻿using Bhbk.Lib.Identity.Data_EF6.Models_TBL;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Tests.Constants;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data_EF6.Tests.RepositoryTests_TBL
{
    [Collection("RepositoryTests_TBL")]
    public class LoginRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Logins_CreateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.Logins.Create(new tbl_Login());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Logins_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateLogins();

            var result = UoW.Logins.Create(
                Mapper.Map<tbl_Login>(new LoginV1()
                {
                    Name = TestDefaultConstants.LoginName,
                    LoginKey = TestDefaultConstants.LoginKey,
                    IsDeletable = true,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Login>();
        }

        [Fact]
        public void Repo_Logins_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Logins.Delete(new tbl_Login());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Logins_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateLogins();

            var login = UoW.Logins.Get(QueryExpressionFactory.GetQueryExpression<tbl_Login>()
                .Where(x => x.Name == TestDefaultConstants.LoginName).ToLambda())
                .Single();

            UoW.Logins.Delete(login);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Logins_GetV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateLogins();

            var results = UoW.Logins.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Login>>();
            results.Count().Should().Be(UoW.Logins.Count());
        }

        [Fact]
        public void Repo_Logins_UpdateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.Logins.Update(new tbl_Login());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Logins_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateLogins();

            var login = UoW.Logins.Get(QueryExpressionFactory.GetQueryExpression<tbl_Login>()
                .Where(x => x.Name == TestDefaultConstants.LoginName).ToLambda())
                .Single();
            login.Name += "(Updated)";

            var result = UoW.Logins.Update(login);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_Login>();
        }
    }
}
