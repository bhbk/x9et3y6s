using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Services
{
    public class StsService : IStsService
    {
        public StsRepository Endpoints { get; }
        public IOAuth2JwtGrant Grant { get; set; }

        public StsService()
            : this(InstanceContext.DeployedOrLocal, new HttpClient())
        { }

        public StsService(InstanceContext instance, HttpClient http)
            : this(new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build(),
                  instance, http)
        { }

        public StsService(IConfiguration conf, InstanceContext instance, HttpClient http)
        {
            Endpoints = new StsRepository(conf, instance, http);
        }

        public JwtSecurityToken Jwt
        {
            get { return Grant.Jwt; }
            set { Grant.Jwt = value; }
        }

        public async ValueTask<AuthCodeV1> AuthCode_AskV1(AuthCodeAskV1 model)
        {
            var response = await Endpoints.AuthCode_AskV1(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<AuthCodeV1>();

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<AuthCodeV2> AuthCode_AskV2(AuthCodeAskV2 model)
        {
            var response = await Endpoints.AuthCode_AskV2(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<AuthCodeV2>();

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserJwtV1> AuthCode_GrantV1(AuthCodeV1 model)
        {
            var response = await Endpoints.AuthCode_AuthV1(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV1>();

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserJwtV2> AuthCode_GrantV2(AuthCodeV2 model)
        {
            var response = await Endpoints.AuthCode_AuthV2(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV2>();

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<ClientJwtV1> ClientCredential_GrantV1(ClientCredentialV1 model)
        {
            var response = await Endpoints.ClientCredential_AuthV1(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<ClientJwtV1>();

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<ClientJwtV2> ClientCredential_GrantV2(ClientCredentialV2 model)
        {
            var response = await Endpoints.ClientCredential_AuthV2(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<ClientJwtV2>();

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<ClientJwtV1> ClientCredential_RefreshV1(RefreshTokenV1 model)
        {
            var response = await Endpoints.ClientCredential_RefreshV1(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<ClientJwtV1>();

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<ClientJwtV2> ClientCredential_RefreshV2(RefreshTokenV2 model)
        {
            var response = await Endpoints.ClientCredential_RefreshV2(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<ClientJwtV2>();

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<DeviceCodeV1> DeviceCode_AskV1(DeviceCodeAskV1 model)
        {
            var response = await Endpoints.DeviceCode_AskV1(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<DeviceCodeV1>();

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<DeviceCodeV2> DeviceCode_AskV2(DeviceCodeAskV2 model)
        {
            var response = await Endpoints.DeviceCode_AskV2(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<DeviceCodeV2>();

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserJwtV1> DeviceCode_GrantV1(DeviceCodeV1 model)
        {
            var response = await Endpoints.DeviceCode_AuthV1(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV1>();

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserJwtV2> DeviceCode_GrantV2(DeviceCodeV2 model)
        {
            var response = await Endpoints.DeviceCode_AuthV2(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV2>();

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserJwtV1> Implicit_GrantV1(ImplicitV1 model)
        {
            var response = await Endpoints.Implicit_AuthV1(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV1>();

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserJwtV2> Implicit_GrantV2(ImplicitV2 model)
        {
            var response = await Endpoints.Implicit_AuthV2(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV2>();

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserJwtV1Legacy> ResourceOwner_GrantV1Legacy(ResourceOwnerV1 model)
        {
            var response = await Endpoints.ResourceOwner_AuthV1Legacy(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV1Legacy>();

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserJwtV1> ResourceOwner_GrantV1(ResourceOwnerV1 model)
        {
            var response = await Endpoints.ResourceOwner_AuthV1(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV1>();

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserJwtV2> ResourceOwner_GrantV2(ResourceOwnerV2 model)
        {
            var response = await Endpoints.ResourceOwner_AuthV2(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV2>();

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserJwtV1> ResourceOwner_RefreshV1(RefreshTokenV1 model)
        {
            var response = await Endpoints.ResourceOwner_RefreshV1(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV1>();

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }

        public async ValueTask<UserJwtV2> ResourceOwner_RefreshV2(RefreshTokenV2 model)
        {
            var response = await Endpoints.ResourceOwner_RefreshV2(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV2>();

            throw new HttpRequestException(response.ToString(),
                new Exception(response.RequestMessage.ToString()));
        }
    }
}
