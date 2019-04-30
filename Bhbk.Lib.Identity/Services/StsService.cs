using Bhbk.Lib.Core.Extensions;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Repositories;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

namespace Bhbk.Lib.Identity.Services
{
    public class StsService : IStsService
    {
        private readonly ResourceOwnerHelper _jwt;

        public StsService()
            : this(InstanceContext.DeployedOrLocal, new HttpClient()) { }

        public StsService(InstanceContext instance, HttpClient client)
        {
            _jwt = new ResourceOwnerHelper(instance, client);
            Http = new StsRepository(instance, client);
        }

        public JwtSecurityToken Jwt
        {
            get { return _jwt.JwtV2; }
            set { _jwt.JwtV2 = value; }
        }

        public StsRepository Http { get; }

        public AuthCodeV1 AuthCode_AskV1(AuthCodeAskV1 model)
        {
            var response = Http.AuthCode_AskV1(model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<AuthCodeV1>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public AuthCodeV2 AuthCode_AskV2(AuthCodeAskV2 model)
        {
            var response = Http.AuthCode_AskV2(model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<AuthCodeV2>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserJwtV1 AuthCode_UseV1(AuthCodeV1 model)
        {
            var response = Http.AuthCode_UseV1(model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserJwtV2 AuthCode_UseV2(AuthCodeV2 model)
        {
            var response = Http.AuthCode_UseV2(model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public ClientJwtV1 ClientCredential_UseV1(ClientCredentialV1 model)
        {
            var response = Http.ClientCredential_UseV1(model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<ClientJwtV1>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public ClientJwtV2 ClientCredential_UseV2(ClientCredentialV2 model)
        {
            var response = Http.ClientCredential_UseV2(model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<ClientJwtV2>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public ClientJwtV1 ClientCredentialRefresh_UseV1(RefreshTokenV1 model)
        {
            var response = Http.ClientCredentialRefresh_UseV1(model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<ClientJwtV1>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public ClientJwtV2 ClientCredentialRefresh_UseV2(RefreshTokenV2 model)
        {
            var response = Http.ClientCredentialRefresh_UseV2(model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<ClientJwtV2>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public DeviceCodeV1 DeviceCode_AskV1(DeviceCodeAskV1 model)
        {
            var response = Http.DeviceCode_AskV1(model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<DeviceCodeV1>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public DeviceCodeV2 DeviceCode_AskV2(DeviceCodeAskV2 model)
        {
            var response = Http.DeviceCode_AskV2(model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<DeviceCodeV2>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserJwtV1 DeviceCode_UseV1(DeviceCodeV1 model)
        {
            var response = Http.DeviceCode_UseV1(model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserJwtV2 DeviceCode_UseV2(DeviceCodeV2 model)
        {
            var response = Http.DeviceCode_UseV2(model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserJwtV1 Implicit_UseV1(ImplicitV1 model)
        {
            var response = Http.Implicit_UseV1(model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserJwtV2 Implicit_UseV2(ImplicitV2 model)
        {
            var response = Http.Implicit_UseV2(model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserJwtV1Legacy ResourceOwner_UseV1Legacy(ResourceOwnerV1 model)
        {
            var response = Http.ResourceOwner_UseV1Legacy(model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserJwtV1Legacy>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserJwtV1 ResourceOwner_UseV1(ResourceOwnerV1 model)
        {
            var response = Http.ResourceOwner_UseV1(model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserJwtV2 ResourceOwner_UseV2(ResourceOwnerV2 model)
        {
            var response = Http.ResourceOwner_UseV2(model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserJwtV1 ResourceOwnerRefresh_UseV1(RefreshTokenV1 model)
        {
            var response = Http.ResourceOwnerRefresh_UseV1(model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserJwtV1>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public UserJwtV2 ResourceOwnerRefresh_UseV2(RefreshTokenV2 model)
        {
            var response = Http.ResourceOwnerRefresh_UseV2(model).Result;

            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsJsonAsync<UserJwtV2>().Result;

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }
    }
}
