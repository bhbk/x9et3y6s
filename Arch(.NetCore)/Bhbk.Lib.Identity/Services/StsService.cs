using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Sts;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Services
{
    public class StsService : IStsService
    {
        public IOAuth2JwtGrant Grant { get; set; }
        public StsServiceRepository Endpoints { get; }

        public StsService()
            : this(new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build(), 
                  InstanceContext.DeployedOrLocal, new HttpClient())
        { }

        public StsService(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient())
        { }

        public StsService(IConfiguration conf, InstanceContext instance, HttpClient http)
        {
            Endpoints = new StsServiceRepository(conf, instance, http);
        }

        public async ValueTask<AuthCodeV1> AuthCode_AskV1(AuthCodeAskV1 model)
        {
            var response = await Endpoints.AuthCode_AskV1(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<AuthCodeV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<AuthCodeV2> AuthCode_AskV2(AuthCodeAskV2 model)
        {
            var response = await Endpoints.AuthCode_AskV2(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<AuthCodeV2>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<UserJwtV1> AuthCode_GrantV1(AuthCodeV1 model)
        {
            var response = await Endpoints.AuthCode_AuthV1(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<UserJwtV2> AuthCode_GrantV2(AuthCodeV2 model)
        {
            var response = await Endpoints.AuthCode_AuthV2(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV2>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<ClientJwtV1> ClientCredential_GrantV1(ClientCredentialV1 model)
        {
            var response = await Endpoints.ClientCredential_AuthV1(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<ClientJwtV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<ClientJwtV2> ClientCredential_GrantV2(ClientCredentialV2 model)
        {
            var response = await Endpoints.ClientCredential_AuthV2(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<ClientJwtV2>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<ClientJwtV1> ClientCredential_RefreshV1(RefreshTokenV1 model)
        {
            var response = await Endpoints.ClientCredential_RefreshV1(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<ClientJwtV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<ClientJwtV2> ClientCredential_RefreshV2(RefreshTokenV2 model)
        {
            var response = await Endpoints.ClientCredential_RefreshV2(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<ClientJwtV2>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<DeviceCodeV1> DeviceCode_AskV1(DeviceCodeAskV1 model)
        {
            var response = await Endpoints.DeviceCode_AskV1(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<DeviceCodeV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<DeviceCodeV2> DeviceCode_AskV2(DeviceCodeAskV2 model)
        {
            var response = await Endpoints.DeviceCode_AskV2(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<DeviceCodeV2>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<UserJwtV1> DeviceCode_GrantV1(DeviceCodeV1 model)
        {
            var response = await Endpoints.DeviceCode_AuthV1(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<UserJwtV2> DeviceCode_GrantV2(DeviceCodeV2 model)
        {
            var response = await Endpoints.DeviceCode_AuthV2(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV2>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<UserJwtV1> Implicit_GrantV1(ImplicitV1 model)
        {
            var response = await Endpoints.Implicit_AuthV1(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<UserJwtV2> Implicit_GrantV2(ImplicitV2 model)
        {
            var response = await Endpoints.Implicit_AuthV2(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV2>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<UserJwtV1Legacy> ResourceOwner_GrantV1Legacy(ResourceOwnerV1 model)
        {
            var response = await Endpoints.ResourceOwner_AuthV1Legacy(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV1Legacy>();

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<UserJwtV1> ResourceOwner_GrantV1(ResourceOwnerV1 model)
        {
            var response = await Endpoints.ResourceOwner_AuthV1(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<UserJwtV2> ResourceOwner_GrantV2(ResourceOwnerV2 model)
        {
            var response = await Endpoints.ResourceOwner_AuthV2(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV2>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<UserJwtV1> ResourceOwner_RefreshV1(RefreshTokenV1 model)
        {
            var response = await Endpoints.ResourceOwner_RefreshV1(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV1>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }

        public async ValueTask<UserJwtV2> ResourceOwner_RefreshV2(RefreshTokenV2 model)
        {
            var response = await Endpoints.ResourceOwner_RefreshV2(model);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<UserJwtV2>().ConfigureAwait(false);

            throw new HttpRequestException(response.RequestMessage.ToString(),
                new Exception(response.ToString()));
        }
    }
}
