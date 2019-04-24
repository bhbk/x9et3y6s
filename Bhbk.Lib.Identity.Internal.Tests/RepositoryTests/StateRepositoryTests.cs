using Bhbk.Lib.Identity.Internal.Models;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Lib.Identity.Internal.Tests.RepositoryTests
{
    [Collection("LibraryTests")]
    public class StateRepositoryTests
    {
        private StartupTests _factory;

        public StateRepositoryTests(StartupTests factory) => _factory = factory;

        [Fact]
        public async Task Lib_StateRepo_GetV1_Success()
        {
            await _factory.DefaultData.DestroyAsync();
            await _factory.DefaultData.CreateAsync();

            var result = await _factory.UoW.StateRepo.GetAsync();

            result.Should().BeAssignableTo<IEnumerable<tbl_States>>();
        }
    }
}
