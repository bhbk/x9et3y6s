using AutoMapper;
using Bhbk.Lib.Identity.Maps;
using Xunit;

namespace Bhbk.Lib.Identity.Internal.Tests.Maps
{
    public class AutoMapperProfileTest
    {
        [Fact]
        public void Lib_CheckAutoMapper_Success()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<ActivityMaps>();
                x.AddProfile<ClientMaps>();
                x.AddProfile<IssuerMaps>();
                x.AddProfile<LoginMaps>();
                x.AddProfile<RoleMaps>();
                x.AddProfile<UserMaps>();
            });
            Mapper.Configuration.AssertConfigurationIsValid();
        }
    }
}
