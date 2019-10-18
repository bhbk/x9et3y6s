using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Domain.Tests.Helpers;
using Bhbk.Lib.Identity.Domain.Tests.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Lib.Identity.Domain.Tests.RepositoryTests
{
    [Collection("LibraryRepositoryTests")]
    public class StateRepositoryTests : BaseRepositoryTests
    {
        [Fact(Skip = "NotImplemented")]
        public async Task Repo_States_CreateV1_Fail()
        {
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
            {
                await UoW.States.CreateAsync(
                    Mapper.Map<tbl_States>(new StateCreate()));
                await UoW.CommitAsync();
            });

            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await UoW.States.CreateAsync(
                    Mapper.Map<tbl_States>(new StateCreate()
                        {
                            IssuerId = Guid.NewGuid(),
                            ClientId = Guid.NewGuid(),
                            UserId = Guid.NewGuid(),
                            StateValue = AlphaNumeric.CreateString(32),
                            StateType = StateType.Device.ToString(),
                            StateConsume = false,
                            ValidFromUtc = DateTime.UtcNow,
                            ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                        }));
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async Task Repo_States_CreateV1_Success()
        {
            new TestData(UoW, Mapper).DestroyAsync().Wait();
            new TestData(UoW, Mapper).CreateAsync().Wait();

            var issuer = (await UoW.Issuers.GetAsync(x => x.Name == Constants.ApiTestIssuer)).Single();
            var client = (await UoW.Clients.GetAsync(x => x.Name == Constants.ApiTestClient)).Single();
            var user = (await UoW.Users.GetAsync(x => x.Email == Constants.ApiTestUser)).Single();

            var result = await UoW.States.CreateAsync(
                Mapper.Map<tbl_States>(new StateCreate()
                    {
                        IssuerId = issuer.Id,
                        ClientId = client.Id,
                        UserId = user.Id,
                        StateValue = AlphaNumeric.CreateString(32),
                        StateType = StateType.Device.ToString(),
                        StateConsume = false,
                        ValidFromUtc = DateTime.UtcNow,
                        ValidToUtc = DateTime.UtcNow.AddSeconds(60),
                    }));
            result.Should().BeAssignableTo<tbl_States>();
            await UoW.CommitAsync();
        }

        [Fact]
        public async Task Repo_States_DeleteV1_Fail()
        {
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            {
                await UoW.States.DeleteAsync(new tbl_States());
                await UoW.CommitAsync();
            });
        }

        [Fact]
        public async Task Repo_States_DeleteV1_Success()
        {
            new TestData(UoW, Mapper).DestroyAsync().Wait();
            new TestData(UoW, Mapper).CreateAsync().Wait();

            var state = (await UoW.States.GetAsync()).First();

            await UoW.States.DeleteAsync(state);
            await UoW.CommitAsync();
        }

        [Fact]
        public async Task Repo_States_GetV1_Success()
        {
            new TestData(UoW, Mapper).DestroyAsync().Wait();
            new TestData(UoW, Mapper).CreateAsync().Wait();

            var result = await UoW.States.GetAsync();
            result.Should().BeAssignableTo<IEnumerable<tbl_States>>();
            await UoW.CommitAsync();
        }

        [Fact]
        public async Task Repo_States_UpdateV1_Fail()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await UoW.States.UpdateAsync(new tbl_States());
                await UoW.CommitAsync();
            });
        }
    }
}
