using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.DomainModels.Alert;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Providers
{
    public class AlertClient : AlertEndpoints
    {
        public AlertClient(IConfigurationRoot conf, ExecutionType situation, HttpClient http)
            : base(conf, situation, http) { }
    }

    public class AlertEndpoints
    {
        protected readonly IConfigurationRoot _conf;
        protected readonly ExecutionType _situation;
        protected readonly HttpClient _http;

        public AlertEndpoints(IConfigurationRoot conf, ExecutionType situation, HttpClient http)
        {
            if (conf == null)
                throw new ArgumentNullException();

            _situation = situation;
            _conf = conf;

            if (situation == ExecutionType.Live)
            {
                var connect = new HttpClientHandler();

                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                _http = new HttpClient(connect);
            }

            if (situation == ExecutionType.UnitTest)
                _http = http;
        }

        public async Task<HttpResponseMessage> Enqueue_ExceptionV1(string jwt, ExceptionCreate model)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/exception/v1";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["AlertUrls:BaseApiUrl"], _conf["AlertUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Enqueue_EmailV1(string jwt, EmailCreate model)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/email/v1";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["AlertUrls:BaseApiUrl"], _conf["AlertUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }

        public async Task<HttpResponseMessage> Enqueue_TextV1(string jwt, TextCreate model)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var endpoint = "/text/v1";

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (_situation == ExecutionType.Live)
                return await _http.PostAsync(
                    string.Format("{0}{1}{2}", _conf["AlertUrls:BaseApiUrl"], _conf["AlertUrls:BaseApiPath"], endpoint), content);

            if (_situation == ExecutionType.UnitTest)
                return await _http.PostAsync(endpoint, content);

            throw new NotSupportedException();
        }
    }
}
