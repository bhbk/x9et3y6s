using Bhbk.Lib.Identity.Internal.Models;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Lib.Identity.Internal.Tests.RepositoryTests
{
    [Collection("LibraryTests")]
    public class ActivityRepositoryTests
    {
        private StartupTests _factory;

        public ActivityRepositoryTests(StartupTests factory) => _factory = factory;

        [Fact]
        public async Task Lib_ActivityRepo_GetV1_Success()
        {
            await _factory.DefaultData.DestroyAsync();
            await _factory.DefaultData.CreateAsync();

            var result = await _factory.UoW.ActivityRepo.GetAsync();

            result.Should().BeAssignableTo<IEnumerable<tbl_Activities>>();
        }
    }
}
