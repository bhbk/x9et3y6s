using Bhbk.Lib.Core.Extensions;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

namespace Bhbk.Lib.Identity.Services
{
    public class StsService : IStsService
    {
        private readonly ResourceOwnerHelper _jwt;
        private readonly StsRepository _repo;

        public StsService(IConfigurationRoot conf, InstanceContext instance, HttpClient client)
        {
            _jwt = new ResourceOwnerHelper(conf, instance, client);
            _repo = new StsRepository(conf, instance, client);
        }

        public JwtSecurityToken Jwt
        {
            get { return _jwt.JwtV2; }
            set { _jwt.JwtV2 = value; }
        }

        public StsRepository Repo
        {
            get { return _repo; }
        }

        public AuthCodeV1 AuthCode_AskV1(AuthCodeAskV1 model)
        {
            var response = _repo.AuthCode_AskV1(model).Result;

            return response.Content.ReadAsJsonAsync<AuthCodeV1>().Result;
        }

        public AuthCodeV2 AuthCode_AskV2(AuthCodeAskV2 model)
        {
            var response = _repo.AuthCode_AskV2(model).Result;

            return response.Content.ReadAsJsonAsync<AuthCodeV2>().Result;
        }

        public UserJwtV1 AuthCode_UseV1(AuthCodeV1 model)
        {
            var response = _repo.AuthCode_UseV1(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;
        }

        public UserJwtV2 AuthCode_UseV2(AuthCodeV2 model)
        {
            var response = _repo.AuthCode_UseV2(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;
        }

        public UserJwtV1 ClientCredential_UseV1(ClientCredentialV1 model)
        {
            var response = _repo.ClientCredential_UseV1(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;
        }

        public UserJwtV2 ClientCredential_UseV2(ClientCredentialV2 model)
        {
            var response = _repo.ClientCredential_UseV2(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;
        }

        public bool DeviceCode_ActionV1(string code, string action)
        {
            var response = _repo.DeviceCode_ActionV1(_jwt.JwtV2.RawData, code, action).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public bool DeviceCode_ActionV2(string code, string action)
        {
            var response = _repo.DeviceCode_ActionV2(_jwt.JwtV2.RawData, code, action).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public DeviceCodeV1 DeviceCode_AskV1(DeviceCodeAskV1 model)
        {
            var response = _repo.DeviceCode_AskV1(model).Result;

            return response.Content.ReadAsJsonAsync<DeviceCodeV1>().Result;
        }

        public DeviceCodeV2 DeviceCode_AskV2(DeviceCodeAskV2 model)
        {
            var response = _repo.DeviceCode_AskV2(model).Result;

            return response.Content.ReadAsJsonAsync<DeviceCodeV2>().Result;
        }

        public UserJwtV1 DeviceCode_UseV1(DeviceCodeV1 model)
        {
            var response = _repo.DeviceCode_UseV1(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;
        }

        public UserJwtV2 DeviceCode_UseV2(DeviceCodeV2 model)
        {
            var response = _repo.DeviceCode_UseV2(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;
        }

        public UserJwtV1 Implicit_UseV1(ImplicitV1 model)
        {
            var response = _repo.Implicit_UseV1(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;
        }

        public UserJwtV2 Implicit_UseV2(ImplicitV2 model)
        {
            var response = _repo.Implicit_UseV2(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;
        }

        public bool RefreshToken_DeleteAllV1(string userValue)
        {
            var response = _repo.RefreshToken_DeleteAllV1(_jwt.JwtV2.RawData, userValue).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public bool RefreshToken_DeleteAllV2(string userValue)
        {
            var response = _repo.RefreshToken_DeleteAllV2(_jwt.JwtV2.RawData, userValue).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public bool RefreshToken_DeleteV1(string userValue, string token)
        {
            var response = _repo.RefreshToken_DeleteV2(_jwt.JwtV2.RawData, userValue, token).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public bool RefreshToken_DeleteV2(string userValue, string token)
        {
            var response = _repo.RefreshToken_DeleteV2(_jwt.JwtV2.RawData, userValue, token).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public IEnumerable<RefreshTokenV1> RefreshToken_GetListV1(string userValue)
        {
            var response = _repo.RefreshToken_GetListV1(_jwt.JwtV2.RawData, userValue).Result;

            return response.Content.ReadAsJsonAsync<IEnumerable<RefreshTokenV1>>().Result;
        }

        public IEnumerable<RefreshTokenV2> RefreshToken_GetListV2(string userValue)
        {
            var response = _repo.RefreshToken_GetListV2(_jwt.JwtV2.RawData, userValue).Result;

            return response.Content.ReadAsJsonAsync<IEnumerable<RefreshTokenV2>>().Result;
        }

        public UserJwtV1 RefreshToken_UseV1(RefreshTokenV1 model)
        {
            var response = _repo.RefreshToken_UseV1(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;
        }

        public UserJwtV2 RefreshToken_UseV2(RefreshTokenV2 model)
        {
            var response = _repo.RefreshToken_UseV2(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;
        }

        public UserJwtV1Legacy ResourceOwner_UseV1Legacy(ResourceOwnerV1 model)
        {
            var response = _repo.ResourceOwner_UseV1Legacy(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1Legacy>().Result;
        }

        public UserJwtV1 ResourceOwner_UseV1(ResourceOwnerV1 model)
        {
            var response = _repo.ResourceOwner_UseV1(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;
        }

        public UserJwtV2 ResourceOwner_UseV2(ResourceOwnerV2 model)
        {
            var response = _repo.ResourceOwner_UseV2(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;
        }
    }
}
