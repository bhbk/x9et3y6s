using Bhbk.Lib.Core.Extensions;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Repositories;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

namespace Bhbk.Lib.Identity.Services
{
    public class StsService : IStsService
    {
        private readonly JwtHelper _jwt;
        private readonly StsRepository _repo;

        public StsService(IConfigurationRoot conf, InstanceContext instance, HttpClient client)
        {
            _jwt = new JwtHelper(conf, instance, client);
            _repo = new StsRepository(conf, instance, client);
        }

        public JwtSecurityToken Jwt
        {
            get { return _jwt.ResourceOwnerV2; }
            set { _jwt.ResourceOwnerV2 = value; }
        }

        public StsRepository Repo
        {
            get { return _repo; }
        }

        public UserJwtV1 AuthCodeGetJwtV1(AuthCodeV1 model)
        {
            var response = _repo.AuthCode_UseV1(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;
        }

        public UserJwtV2 AuthCodeGetJwtV2(AuthCodeV2 model)
        {
            var response = _repo.AuthCode_UseV2(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;
        }

        public AuthCodeV1 AuthCodeGetV1(AuthCodeAskV1 model)
        {
            var response = _repo.AuthCode_AskV1(model).Result;

            return response.Content.ReadAsJsonAsync<AuthCodeV1>().Result;
        }

        public AuthCodeV2 AuthCodeGetV2(AuthCodeAskV2 model)
        {
            var response = _repo.AuthCode_AskV2(model).Result;

            return response.Content.ReadAsJsonAsync<AuthCodeV2>().Result;
        }

        public UserJwtV1 ClientCredGetJwtV1(ClientCredentialV1 model)
        {
            var response = _repo.ClientCredential_UseV1(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;
        }

        public UserJwtV2 ClientCredGetJwtV2(ClientCredentialV2 model)
        {
            var response = _repo.ClientCredential_UseV2(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;
        }

        public void DeviceCodeDecisionV1(string code, string action)
        {
            var response = _repo.DeviceCode_DecideV1(_jwt.ResourceOwnerV2.RawData, code, action).Result;
        }

        public void DeviceCodeDecisionV2(string code, string action)
        {
            var response = _repo.DeviceCode_DecideV2(_jwt.ResourceOwnerV2.RawData, code, action).Result;
        }

        public UserJwtV1 DeviceCodeGetJwtV1(DeviceCodeV1 model)
        {
            var response = _repo.DeviceCode_UseV1(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;
        }

        public UserJwtV2 DeviceCodeGetJwtV2(DeviceCodeV2 model)
        {
            var response = _repo.DeviceCode_UseV2(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;
        }

        public DeviceCodeV1 DeviceCodeGetV1(DeviceCodeAskV1 model)
        {
            var response = _repo.DeviceCode_AskV1(model).Result;

            return response.Content.ReadAsJsonAsync<DeviceCodeV1>().Result;
        }

        public DeviceCodeV2 DeviceCodeGetV2(DeviceCodeAskV2 model)
        {
            var response = _repo.DeviceCode_AskV2(model).Result;

            return response.Content.ReadAsJsonAsync<DeviceCodeV2>().Result;
        }

        public UserJwtV1 ImplicitGetJwtV1(ImplicitV1 model)
        {
            var response = _repo.Implicit_UseV1(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;
        }

        public UserJwtV2 ImplicitGetJwtV2(ImplicitV2 model)
        {
            var response = _repo.Implicit_UseV2(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;
        }

        public void RefreshTokenDeleteV1(string userValue, string token)
        {
            var response = _repo.RefreshToken_DeleteV2(_jwt.ResourceOwnerV2.RawData, userValue, token).Result;
        }

        public void RefreshTokenDeleteV2(string userValue, string token)
        {
            var response = _repo.RefreshToken_DeleteV2(_jwt.ResourceOwnerV2.RawData, userValue, token).Result;
        }

        public UserJwtV1 RefreshTokenGetJwtV1(RefreshTokenV1 model)
        {
            var response = _repo.RefreshToken_UseV1(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;
        }

        public UserJwtV2 RefreshTokenGetJwtV2(RefreshTokenV2 model)
        {
            var response = _repo.RefreshToken_UseV2(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;
        }

        public void RefreshTokensDeleteV1(string userValue)
        {
            var response = _repo.RefreshToken_DeleteV1(_jwt.ResourceOwnerV2.RawData, userValue).Result;
        }

        public void RefreshTokensDeleteV2(string userValue)
        {
            var response = _repo.RefreshToken_DeleteV2(_jwt.ResourceOwnerV2.RawData, userValue).Result;
        }

        public IEnumerable<RefreshTokenV1> RefreshTokensGetV1(string userValue)
        {
            var response = _repo.RefreshToken_GetListV1(_jwt.ResourceOwnerV2.RawData, userValue).Result;

            return response.Content.ReadAsJsonAsync<IEnumerable<RefreshTokenV1>>().Result;
        }

        public IEnumerable<RefreshTokenV2> RefreshTokensGetV2(string userValue)
        {
            var response = _repo.RefreshToken_GetListV2(_jwt.ResourceOwnerV2.RawData, userValue).Result;

            return response.Content.ReadAsJsonAsync<IEnumerable<RefreshTokenV2>>().Result;
        }

        public UserJwtV1Legacy ResourceOwnerGetJwtV1Legacy(ResourceOwnerV1 model)
        {
            var response = _repo.ResourceOwner_UseV1Legacy(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1Legacy>().Result;
        }

        public UserJwtV1 ResourceOwnerGetJwtV1(ResourceOwnerV1 model)
        {
            var response = _repo.ResourceOwner_UseV1(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;
        }

        public UserJwtV2 ResourceOwnerGetJwtV2(ResourceOwnerV2 model)
        {
            var response = _repo.ResourceOwner_UseV2(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;
        }
    }
}
