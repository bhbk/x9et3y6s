using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Repositories;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace Bhbk.Lib.Identity.Services
{
    public interface IStsService
    {
        JwtSecurityToken Jwt { get; set; }
        StsRepository Repo { get; }

        //authorization code flows
        AuthCodeV1 AuthCodeGetV1(AuthCodeAskV1 model);
        AuthCodeV2 AuthCodeGetV2(AuthCodeAskV2 model);
        UserJwtV1 AuthCodeGetJwtV1(AuthCodeV1 model);
        UserJwtV2 AuthCodeGetJwtV2(AuthCodeV2 model);

        //device code flows
        void DeviceCodeDecisionV1(string code, string action);
        void DeviceCodeDecisionV2(string code, string action);
        DeviceCodeV1 DeviceCodeGetV1(DeviceCodeAskV1 model);
        DeviceCodeV2 DeviceCodeGetV2(DeviceCodeAskV2 model);
        UserJwtV1 DeviceCodeGetJwtV1(DeviceCodeV1 model);
        UserJwtV2 DeviceCodeGetJwtV2(DeviceCodeV2 model);

        //client credential flows
        UserJwtV1 ClientCredGetJwtV1(ClientCredentialV1 model);
        UserJwtV2 ClientCredGetJwtV2(ClientCredentialV2 model);

        //implicit flows
        UserJwtV1 ImplicitGetJwtV1(ImplicitV1 model);
        UserJwtV2 ImplicitGetJwtV2(ImplicitV2 model);

        //refresh tokens
        void RefreshTokenDeleteV1(string userValue, string token);
        void RefreshTokenDeleteV2(string userValue, string token);
        void RefreshTokensDeleteV1(string userValue);
        void RefreshTokensDeleteV2(string userValue);
        IEnumerable<RefreshTokenV1> RefreshTokensGetV1(string userValue);
        IEnumerable<RefreshTokenV2> RefreshTokensGetV2(string userValue);
        UserJwtV1 RefreshTokenGetJwtV1(RefreshTokenV1 model);
        UserJwtV2 RefreshTokenGetJwtV2(RefreshTokenV2 model);

        //resource owner flows
        UserJwtV1Legacy ResourceOwnerGetJwtV1Legacy(ResourceOwnerV1 model);
        UserJwtV1 ResourceOwnerGetJwtV1(ResourceOwnerV1 model);
        UserJwtV2 ResourceOwnerGetJwtV2(ResourceOwnerV2 model);
    }
}
