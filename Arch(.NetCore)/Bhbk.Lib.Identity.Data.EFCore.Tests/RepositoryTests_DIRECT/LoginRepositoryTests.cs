using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Lib.Identity.Data.EFCore.Tests.RepositoryTests_DIRECT
{
    [Collection("RepositoryTests_DIRECT")]
    public class LoginRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public void Repo_Logins_CreateV1_Fail()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                UoW.Logins.Create(
                    Mapper.Map<tbl_Logins>(new LoginV1()));
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Logins_CreateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var result = UoW.Logins.Create(
                Mapper.Map<tbl_Logins>(new LoginV1()
                {
                    Name = Constants.ApiTestLogin,
                    Immutable = false,
                }));
            result.Should().BeAssignableTo<tbl_Logins>();

            UoW.Commit();
        }

        [Fact]
        public void Repo_Logins_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.Logins.Delete(new tbl_Logins());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Logins_DeleteV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var login = UoW.Logins.Get(new QueryExpression<tbl_Logins>()
                .Where(x => x.Name == Constants.ApiTestLogin).ToLambda())
                .Single();

            UoW.Logins.Delete(login);
            UoW.Commit();
        }

        [Fact]
        public void Repo_Logins_GetV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var results = UoW.Logins.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_Logins>>();
            results.Count().Should().Be(UoW.Logins.Count());
        }

        [Fact]
        public void Repo_Logins_UpdateV1_Fail()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                UoW.Logins.Update(new tbl_Logins());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_Logins_UpdateV1_Success()
        {
            new GenerateTestData(UoW, Mapper).Destroy();
            new GenerateTestData(UoW, Mapper).Create();

            var login = UoW.Logins.Get(new QueryExpression<tbl_Logins>()
                .Where(x => x.Name == Constants.ApiTestLogin).ToLambda())
                .Single();
            login.Name += "(Updated)";

            var result = UoW.Logins.Update(login);
            result.Should().BeAssignableTo<tbl_Logins>();

            UoW.Commit();
        }
    }
}
