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
        AuthCodeV1 AuthCode_AskV1(AuthCodeAskV1 model);
        AuthCodeV2 AuthCode_AskV2(AuthCodeAskV2 model);
        UserJwtV1 AuthCode_UseV1(AuthCodeV1 model);
        UserJwtV2 AuthCode_UseV2(AuthCodeV2 model);

        //device code flows
        bool DeviceCode_ActionV1(string code, string action);
        bool DeviceCode_ActionV2(string code, string action);
        DeviceCodeV1 DeviceCode_AskV1(DeviceCodeAskV1 model);
        DeviceCodeV2 DeviceCode_AskV2(DeviceCodeAskV2 model);
        UserJwtV1 DeviceCode_UseV1(DeviceCodeV1 model);
        UserJwtV2 DeviceCode_UseV2(DeviceCodeV2 model);

        //client credential flows
        ClientJwtV1 ClientCredential_UseV1(ClientCredentialV1 model);
        ClientJwtV2 ClientCredential_UseV2(ClientCredentialV2 model);
        ClientJwtV1 ClientCredentialRefresh_UseV1(RefreshTokenV1 model);
        ClientJwtV2 ClientCredentialRefresh_UseV2(RefreshTokenV2 model);

        //implicit flows
        UserJwtV1 Implicit_UseV1(ImplicitV1 model);
        UserJwtV2 Implicit_UseV2(ImplicitV2 model);

        //refresh tokens
        bool RefreshToken_DeleteV1(string userValue, string token);
        bool RefreshToken_DeleteV2(string userValue, string token);
        bool RefreshToken_DeleteAllV1(string userValue);
        bool RefreshToken_DeleteAllV2(string userValue);
        IEnumerable<RefreshTokenV1> RefreshToken_GetListV1(string userValue);
        IEnumerable<RefreshTokenV2> RefreshToken_GetListV2(string userValue);

        //resource owner flows
        UserJwtV1Legacy ResourceOwner_UseV1Legacy(ResourceOwnerV1 model);
        UserJwtV1 ResourceOwner_UseV1(ResourceOwnerV1 model);
        UserJwtV2 ResourceOwner_UseV2(ResourceOwnerV2 model);
        UserJwtV1 ResourceOwnerRefresh_UseV1(RefreshTokenV1 model);
        UserJwtV2 ResourceOwnerRefresh_UseV2(RefreshTokenV2 model);
    }
}
