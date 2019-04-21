﻿using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Domain.Tests.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Lib.Identity.Domain.Tests.RepositoryTests
{
    [Collection("LibraryTestsCollection")]
    public class UserRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async Task Lib_UserRepo_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.UserRepo.CreateAsync(
                    Mapper.Map<tbl_Users>(new UserCreate()));
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.UserRepo.CreateAsync(
                    Mapper.Map<tbl_Users>(new UserCreate()
                        {
                            Email = AlphaNumeric.CreateString(4),
                            PhoneNumber = Constants.ApiTestUserPhone,
                            FirstName = "First-" + Base64.CreateString(4),
                            LastName = "Last-" + Base64.CreateString(4),
                            LockoutEnabled = false,
                            HumanBeing = true,
                            Immutable = false,
                        }));
            });
        }

        [Fact]
        public async Task Lib_UserRepo_CreateV1_Success()
        {
            TestData.DestroyAsync().Wait();
            TestData.CreateAsync().Wait();

            var result = await UoW.UserRepo.CreateAsync(
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
        }

        [Fact]
        public async Task Lib_UserRepo_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.UserRepo.DeleteAsync(Guid.NewGuid());
            });
        }

        [Fact]
        public async Task Lib_UserRepo_DeleteV1_Success()
        {
            TestData.DestroyAsync().Wait();
            TestData.CreateAsync().Wait();

            var issuer = (await UoW.UserRepo.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();

            var result = await UoW.UserRepo.DeleteAsync(issuer.Id);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Lib_UserRepo_GetV1_Success()
        {
            TestData.DestroyAsync().Wait();
            TestData.CreateAsync().Wait();

            var result = await UoW.UserRepo.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_Users>>();
        }

        [Fact]
        public async Task Lib_UserRepo_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await UoW.UserRepo.UpdateAsync(new tbl_Users());
            });
        }

        [Fact]
        public async Task Lib_UserRepo_UpdateV1_Success()
        {
            TestData.DestroyAsync().Wait();
            TestData.CreateAsync().Wait();

            var user = (await UoW.UserRepo.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();
            user.FirstName += "(Updated)";
            user.LastName += "(Updated)";

            var result = await UoW.UserRepo.UpdateAsync(user);

            result.Should().BeAssignableTo<tbl_Users>();
        }
    }
}
