using Bhbk.Lib.Identity.Internal.Models;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Lib.Identity.Internal.Tests.RepositoryTests
{
    [Collection("LibraryTests")]
    public class ClaimRepositoryTests
    {
        private StartupTests _factory;

        public ClaimRepositoryTests(StartupTests startup) => _factory = startup;

        [Fact]
        public async Task Lib_ClaimRepo_GetV1_Success()
        {
            await _factory.DefaultData.DestroyAsync();
            await _factory.DefaultData.CreateAsync();

            var result = await _factory.UoW.ClaimRepo.GetAsync();

            result.Should().BeAssignableTo<IEnumerable<tbl_Claims>>();
        }
    }
}
