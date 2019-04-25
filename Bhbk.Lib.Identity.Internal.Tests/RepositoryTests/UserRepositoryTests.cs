using Bhbk.Lib.Identity.Internal.Models;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Lib.Identity.Internal.Tests.RepositoryTests
{
    [Collection("LibraryTests")]
    public class UserRepositoryTests
    {
        private StartupTests _factory;

        public UserRepositoryTests(StartupTests startup) => _factory = startup;

        [Fact]
        public async Task Lib_UserRepo_GetV1_Success()
        {
            await _factory.DefaultData.DestroyAsync();
            await _factory.DefaultData.CreateAsync();

            var result = await _factory.UoW.UserRepo.GetAsync();

            result.Should().BeAssignableTo<IEnumerable<tbl_Users>>();
        }
    }
}
