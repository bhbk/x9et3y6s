using AutoMapper;
using Bhbk.Lib.Identity.Internal.Infrastructure;
using Xunit;

namespace Bhbk.Lib.Identity.Internal.Tests.Libraries
{
    public class AutoMapperLibraryTest
    {
        [Fact]
        public void Lib_AutoMapper_Success()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<IdentityMapper>();
            });
            Mapper.Configuration.AssertConfigurationIsValid();
        }
    }
}
