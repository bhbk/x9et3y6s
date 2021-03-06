﻿using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data_EF6.Models;
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

namespace Bhbk.Lib.Identity.Data_EF6.Tests.RepositoryTests
{
    [Collection("RepositoryTests")]
    public class UserRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_Users_CreateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.Users.Create(new E_User());
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
                Mapper.Map<E_User>(new UserV1()
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

            result.Should().BeAssignableTo<E_User>();
        }

        [Fact]
        public void Repo_Users_DeleteV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Users.Delete(new E_User());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Users_DeleteV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateUsers();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<E_User>()
                .Where(x => x.UserName == TestDefaultConstants.UserName).ToLambda()).Single();

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
            results.Should().BeAssignableTo<IEnumerable<E_User>>();
            results.Count().Should().Be(UoW.Users.Count());
        }

        [Fact]
        public void Repo_Users_UpdateV1_Fail()
        {
            Assert.Throws<DbEntityValidationException>(() =>
            {
                UoW.Users.Update(new E_User());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Users_UpdateV1_Success()
        {
            var data = new TestDataFactory(UoW);
            data.Destroy();
            data.CreateUsers();

            var user = UoW.Users.Get(QueryExpressionFactory.GetQueryExpression<E_User>()
                .Where(x => x.UserName == TestDefaultConstants.UserName).ToLambda()).Single();
            user.FirstName += "(Updated)";
            user.LastName += "(Updated)";

            var result = UoW.Users.Update(user);
            UoW.Commit();

            result.Should().BeAssignableTo<E_User>();
        }
    }
}
