﻿using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data_EF6.Models_Tbl;
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

namespace Bhbk.Lib.Identity.Data_EF6.Tests.RepositoryTests_Tbl
{
    [Collection("RepositoryTests_Tbl")]
    public class UserRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Users_CreateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.Users.Create(new tbl_User());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Users_CreateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateUsers();

            var result = UoW.Users.Create(
                Mapper.Map<tbl_User>(new UserV1()
                {
                    UserName = TestDefaultConstants.UserName,
                    Email = TestDefaultConstants.UserName,
                    PhoneNumber = NumberAs.CreateString(9),
                    FirstName = "First-" + Base64.CreateString(4),
                    LastName = "Last-" + Base64.CreateString(4),
                    IsLockedOut = false,
                    IsHumanBeing = true,
                    IsDeletable = true,
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_User>();
        }

        [Fact]
        public void Repo_Users_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Users.Delete(new tbl_User());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Users_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateUsers();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.UserName == TestDefaultConstants.UserName).ToLambda()).Single();

            UoW.AuthActivity.Delete(QueryExpressionFactory.GetQueryExpression<tbl_AuthActivity>()
                .Where(x => x.UserId == user.Id).ToLambda());

            UoW.Refreshes.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.UserId == user.Id).ToLambda());

            UoW.Settings.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Setting>()
                .Where(x => x.UserId == user.Id).ToLambda());

            UoW.States.Delete(QueryExpressionFactory.GetQueryExpression<tbl_State>()
                .Where(x => x.UserId == user.Id).ToLambda());

            UoW.Users.Delete(user);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Users_GetV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateUsers();

            var results = UoW.Users.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_User>>();
            results.Count().Should().Be(UoW.Users.Count());
        }

        [Fact]
        public void Repo_Users_UpdateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.Users.Update(new tbl_User());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Users_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateUsers();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.UserName == TestDefaultConstants.UserName).ToLambda()).Single();
            user.FirstName += "(Updated)";
            user.LastName += "(Updated)";

            var result = UoW.Users.Update(user);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_User>();
        }
    }
}
