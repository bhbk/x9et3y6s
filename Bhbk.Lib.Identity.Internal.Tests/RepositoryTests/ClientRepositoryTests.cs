using Bhbk.Lib.Identity.Internal.Models;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Bhbk.Lib.Identity.Internal.Tests.RepositoryTests
{
    [Collection("LibraryTests")]
    public class ClientRepositoryTests
    {
        private StartupTests _factory;

        public ClientRepositoryTests(StartupTests startup) => _factory = startup;

        [Fact]
        public async Task Lib_ClientRepo_GetV1_Success()
        {
            await _factory.DefaultData.DestroyAsync();
            await _factory.DefaultData.CreateAsync();

            var result = await _factory.UoW.ClientRepo.GetAsync();

            result.Should().BeAssignableTo<IEnumerable<tbl_Clients>>();
        }
    }
}
