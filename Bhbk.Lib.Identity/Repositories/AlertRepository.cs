﻿using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Alert;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Repositories
{
    public class AlertRepository
    {
        private readonly IConfigurationRoot _conf;
        private readonly InstanceContext _instance;
        private readonly HttpClient _client;

        public AlertRepository(IConfigurationRoot conf, InstanceContext instance, HttpClient client)
        {
            _conf = conf ?? throw new ArgumentNullException();
            _instance = instance;

            if (instance == InstanceContext.DeployedOrLocal)
            {
                var connect = new HttpClientHandler();

                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                _client = new HttpClient(connect);
            }

            if (instance == InstanceContext.UnitTest)
                _client = client;

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<HttpResponseMessage> Enqueue_EmailV1(string jwt, EmailCreate model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/email/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["AlertUrls:BaseApiUrl"], _conf["AlertUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Enqueue_TextV1(string jwt, TextCreate model)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var endpoint = "/text/v1";

            if (_instance == InstanceContext.DeployedOrLocal)
                return await _client.PostAsync(string.Format("{0}{1}{2}", _conf["AlertUrls:BaseApiUrl"], _conf["AlertUrls:BaseApiPath"], endpoint), content);

            if (_instance == InstanceContext.UnitTest)
                return await _client.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }
    }
}