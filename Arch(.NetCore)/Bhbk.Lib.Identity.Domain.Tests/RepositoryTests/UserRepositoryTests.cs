using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Models;
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
    public class UserRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public void Repo_Users_CreateV1_Fail()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                UoW.Users.Create(
                    Mapper.Map<tbl_Users>(new UserCreate()));
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Users_CreateV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var result = UoW.Users.Create(
                Mapper.Map<tbl_Users>(new UserCreate()
                    {
                        Email = Constants.ApiTestUser,
                        PhoneNumber = Constants.ApiTestUserPhone,
                        FirstName = "First-" + Base64.CreateString(4),
                        LastName = "Last-" + Base64.CreateString(4),
                        LockoutEnabled = false,
                        HumanBeing = true,
                        Immutable = false,
                    }));
            result.Should().BeAssignableTo<tbl_Users>();
            UoW.Commit();
        }

        [Fact]
        public void Repo_Users_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Users.Delete(new tbl_Users());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Users_DeleteV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var issuer = UoW.Users.Get(x => x.Email == Constants.ApiTestUser).Single();

            UoW.Users.Delete(issuer);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Users_GetV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var results = UoW.Users.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Users>>();
        }

        [Fact]
        public void Repo_Users_UpdateV1_Fail()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                UoW.Users.Update(new tbl_Users());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Users_UpdateV1_Success()
        {
            new TestData(UoW, Mapper).Destroy();
            new TestData(UoW, Mapper).Create();

            var user = UoW.Users.Get(x => x.Email == Constants.ApiTestUser).Single();
            user.FirstName += "(Updated)";
            user.LastName += "(Updated)";

            var result = UoW.Users.Update(user);

            result.Should().BeAssignableTo<tbl_Users>();
            UoW.Commit();
        }
    }
}
