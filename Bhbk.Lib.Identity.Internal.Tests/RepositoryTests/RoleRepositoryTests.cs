using Bhbk.Lib.Identity.Internal.Models;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Lib.Identity.Internal.Tests.RepositoryTests
{
    [Collection("LibraryTests")]
    public class RoleRepositoryTests
    {
        private StartupTests _factory;

        public RoleRepositoryTests(StartupTests startup) => _factory = startup;

        [Fact]
        public async Task Lib_RoleRepo_GetV1_Success()
        {
            await _factory.DefaultData.DestroyAsync();
            await _factory.DefaultData.CreateAsync();

            var result = await _factory.UoW.RoleRepo.GetAsync();

            result.Should().BeAssignableTo<IEnumerable<tbl_Roles>>();
        }
    }
}
