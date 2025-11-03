using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Domain.Infrastructure;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bhbk.Test.Identity.Integration.Repositories
{
    [Collection("RepositoryTests")]
    public class StateRepositoryTests : BaseRepositoryTests
    {
        [Fact]
        public void Repo_States_CreateV1_Fail()
        {
            Assert.Throws<DbUpdateException>(() =>
            {
                UoW.States.Post(new tbl_State());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_States_CreateV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults())
                .WithUser(u => u.WithDefaults())
                .Seed();

            var result = UoW.States.Post(
                Mapper.Map<tbl_State>(new StateV1()
                {
                    IssuerId = seed.Issuer.Id,
                    AudienceId = seed.Audiences.Values.First().Id,
                    UserId = seed.Users.Values.First().Id,
                    StateValue = AlphaNumeric.CreateString(32),
                    StateType = ConsumerType.Device.ToString(),
                    StateConsume = false,
                    ValidFrom = DateTime.UtcNow,
                    ValidTo = DateTime.UtcNow.AddSeconds(60),
                }));
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_State>();

            UoW.States.Delete(result);
            UoW.Commit();
        }

        [Fact]
        public void Repo_States_DeleteV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.States.Delete(new tbl_State());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_States_DeleteV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults())
                .WithUser(u => u.WithDefaults())
                .Seed();

            var state = UoW.States.Post(
                Mapper.Map<tbl_State>(new StateV1()
                {
                    IssuerId = seed.Issuer.Id,
                    AudienceId = seed.Audiences.Values.First().Id,
                    UserId = seed.Users.Values.First().Id,
                    StateValue = AlphaNumeric.CreateString(32),
                    StateType = ConsumerType.Device.ToString(),
                    StateConsume = false,
                    ValidFrom = DateTime.UtcNow,
                    ValidTo = DateTime.UtcNow.AddSeconds(60),
                }));
            UoW.Commit();

            UoW.States.Delete(state);
            UoW.Commit();
        }

        [Fact]
        public void Repo_States_GetV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults())
                .WithUser(u => u.WithDefaults())
                .Seed();

            UoW.States.Post(
                Mapper.Map<tbl_State>(new StateV1()
                {
                    IssuerId = seed.Issuer.Id,
                    AudienceId = seed.Audiences.Values.First().Id,
                    UserId = seed.Users.Values.First().Id,
                    StateValue = AlphaNumeric.CreateString(32),
                    StateType = ConsumerType.Device.ToString(),
                    StateConsume = false,
                    ValidFrom = DateTime.UtcNow,
                    ValidTo = DateTime.UtcNow.AddSeconds(60),
                }));
            UoW.Commit();

            var results = UoW.States.Get();
            results.Should().BeAssignableTo<IEnumerable<tbl_State>>();
            results.Count().Should().Be(UoW.States.Count());

            UoW.States.Delete(results);
            UoW.Commit();
        }

        [Fact]
        public void Repo_States_UpdateV1_Fail()
        {
            Assert.Throws<DbUpdateConcurrencyException>(() =>
            {
                UoW.States.Put(new tbl_State());
                UoW.Commit();
            });
        }

        [Fact]
        public void Repo_States_UpdateV1_Success()
        {
            using var seed = new SeedContext(UoW)
                .WithIssuer(i => i.WithDefaults())
                .WithAudience(a => a.WithDefaults())
                .WithUser(u => u.WithDefaults())
                .Seed();

            var state = UoW.States.Post(
                Mapper.Map<tbl_State>(new StateV1()
                {
                    IssuerId = seed.Issuer.Id,
                    AudienceId = seed.Audiences.Values.First().Id,
                    UserId = seed.Users.Values.First().Id,
                    StateValue = AlphaNumeric.CreateString(32),
                    StateType = ConsumerType.Device.ToString(),
                    StateConsume = false,
                    ValidFrom = DateTime.UtcNow,
                    ValidTo = DateTime.UtcNow.AddSeconds(60),
                }));
            UoW.Commit();

            state.StateConsume = true;

            var result = UoW.States.Put(state);
            UoW.Commit();

            result.Should().BeAssignableTo<tbl_State>();

            UoW.States.Delete(result);
            UoW.Commit();
        }
    }
}
