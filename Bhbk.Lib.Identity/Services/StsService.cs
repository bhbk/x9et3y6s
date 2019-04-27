using Bhbk.Lib.Core.Extensions;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

namespace Bhbk.Lib.Identity.Services
{
    public class StsService : IStsService
    {
        private readonly ResourceOwnerHelper _jwt;

        public StsService(IConfigurationRoot conf, InstanceContext instance, HttpClient client)
        {
            _jwt = new ResourceOwnerHelper(conf, instance, client);
            Endpoints = new StsRepository(conf, instance, client);
        }

        public JwtSecurityToken Jwt
        {
            get { return _jwt.JwtV2; }
            set { _jwt.JwtV2 = value; }
        }

        public StsRepository Endpoints { get; }

        public AuthCodeV1 AuthCode_AskV1(AuthCodeAskV1 model)
        {
            var response = Endpoints.AuthCode_AskV1(model).Result;

            return response.Content.ReadAsJsonAsync<AuthCodeV1>().Result;
        }

        public AuthCodeV2 AuthCode_AskV2(AuthCodeAskV2 model)
        {
            var response = Endpoints.AuthCode_AskV2(model).Result;

            return response.Content.ReadAsJsonAsync<AuthCodeV2>().Result;
        }

        public UserJwtV1 AuthCode_UseV1(AuthCodeV1 model)
        {
            var response = Endpoints.AuthCode_UseV1(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;
        }

        public UserJwtV2 AuthCode_UseV2(AuthCodeV2 model)
        {
            var response = Endpoints.AuthCode_UseV2(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;
        }

        public ClientJwtV1 ClientCredential_UseV1(ClientCredentialV1 model)
        {
            var response = Endpoints.ClientCredential_UseV1(model).Result;

            return response.Content.ReadAsJsonAsync<ClientJwtV1>().Result;
        }

        public ClientJwtV2 ClientCredential_UseV2(ClientCredentialV2 model)
        {
            var response = Endpoints.ClientCredential_UseV2(model).Result;

            return response.Content.ReadAsJsonAsync<ClientJwtV2>().Result;
        }

        public ClientJwtV1 ClientCredentialRefresh_UseV1(RefreshTokenV1 model)
        {
            var response = Endpoints.ClientCredentialRefresh_UseV1(model).Result;

            return response.Content.ReadAsJsonAsync<ClientJwtV1>().Result;
        }

        public ClientJwtV2 ClientCredentialRefresh_UseV2(RefreshTokenV2 model)
        {
            var response = Endpoints.ClientCredentialRefresh_UseV2(model).Result;

            return response.Content.ReadAsJsonAsync<ClientJwtV2>().Result;
        }

        public DeviceCodeV1 DeviceCode_AskV1(DeviceCodeAskV1 model)
        {
            var response = Endpoints.DeviceCode_AskV1(model).Result;

            return response.Content.ReadAsJsonAsync<DeviceCodeV1>().Result;
        }

        public DeviceCodeV2 DeviceCode_AskV2(DeviceCodeAskV2 model)
        {
            var response = Endpoints.DeviceCode_AskV2(model).Result;

            return response.Content.ReadAsJsonAsync<DeviceCodeV2>().Result;
        }

        public UserJwtV1 DeviceCode_UseV1(DeviceCodeV1 model)
        {
            var response = Endpoints.DeviceCode_UseV1(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;
        }

        public UserJwtV2 DeviceCode_UseV2(DeviceCodeV2 model)
        {
            var response = Endpoints.DeviceCode_UseV2(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;
        }

        public UserJwtV1 Implicit_UseV1(ImplicitV1 model)
        {
            var response = Endpoints.Implicit_UseV1(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;
        }

        public UserJwtV2 Implicit_UseV2(ImplicitV2 model)
        {
            var response = Endpoints.Implicit_UseV2(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;
        }

        public UserJwtV1Legacy ResourceOwner_UseV1Legacy(ResourceOwnerV1 model)
        {
            var response = Endpoints.ResourceOwner_UseV1Legacy(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1Legacy>().Result;
        }

        public UserJwtV1 ResourceOwner_UseV1(ResourceOwnerV1 model)
        {
            var response = Endpoints.ResourceOwner_UseV1(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;
        }

        public UserJwtV2 ResourceOwner_UseV2(ResourceOwnerV2 model)
        {
            var response = Endpoints.ResourceOwner_UseV2(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;
        }

        public UserJwtV1 ResourceOwnerRefresh_UseV1(RefreshTokenV1 model)
        {
            var response = Endpoints.ResourceOwnerRefresh_UseV1(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;
        }

        public UserJwtV2 ResourceOwnerRefresh_UseV2(RefreshTokenV2 model)
        {
            var response = Endpoints.ResourceOwnerRefresh_UseV2(model).Result;

            return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;
        }
    }
}
