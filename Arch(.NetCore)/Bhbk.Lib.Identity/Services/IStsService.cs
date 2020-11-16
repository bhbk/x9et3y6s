using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Services
{
    public interface IStsService
    {
        JwtSecurityToken Jwt { get; set; }
        StsRepository Endpoints { get; }
        IOAuth2JwtGrant Grant { get; set; }

        /*
         * authorization code flows
         */
        ValueTask<AuthCodeV1> AuthCode_AskV1(AuthCodeAskV1 model);
        ValueTask<AuthCodeV2> AuthCode_AskV2(AuthCodeAskV2 model);
        ValueTask<UserJwtV1> AuthCode_GrantV1(AuthCodeV1 model);
        ValueTask<UserJwtV2> AuthCode_GrantV2(AuthCodeV2 model);

        /*
         * device code flows
         */
        ValueTask<DeviceCodeV1> DeviceCode_AskV1(DeviceCodeAskV1 model);
        ValueTask<DeviceCodeV2> DeviceCode_AskV2(DeviceCodeAskV2 model);
        ValueTask<UserJwtV1> DeviceCode_GrantV1(DeviceCodeV1 model);
        ValueTask<UserJwtV2> DeviceCode_GrantV2(DeviceCodeV2 model);

        /*
         * client credential flows
         */
        ValueTask<ClientJwtV1> ClientCredential_GrantV1(ClientCredentialV1 model);
        ValueTask<ClientJwtV2> ClientCredential_GrantV2(ClientCredentialV2 model);
        ValueTask<ClientJwtV1> ClientCredential_RefreshV1(RefreshTokenV1 model);
        ValueTask<ClientJwtV2> ClientCredential_RefreshV2(RefreshTokenV2 model);

        /*
         * implicit flows
         */
        ValueTask<UserJwtV1> Implicit_GrantV1(ImplicitV1 model);
        ValueTask<UserJwtV2> Implicit_GrantV2(ImplicitV2 model);

        /*
         * resource owner flows
         */
        ValueTask<UserJwtV1Legacy> ResourceOwner_GrantV1Legacy(ResourceOwnerV1 model);
        ValueTask<UserJwtV1> ResourceOwner_GrantV1(ResourceOwnerV1 model);
        ValueTask<UserJwtV2> ResourceOwner_GrantV2(ResourceOwnerV2 model);
        ValueTask<UserJwtV1> ResourceOwner_RefreshV1(RefreshTokenV1 model);
        ValueTask<UserJwtV2> ResourceOwner_RefreshV2(RefreshTokenV2 model);
    }
}
