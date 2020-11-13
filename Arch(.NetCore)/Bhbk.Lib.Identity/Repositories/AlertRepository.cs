using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Alert;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace Bhbk.Lib.Identity.Repositories
{
    public class AlertRepository
    {
        private readonly IConfiguration _conf;
        private readonly HttpClient _http;

        public AlertRepository(IConfiguration conf)
            : this(conf, InstanceContext.DeployedOrLocal, new HttpClient()) { }

        public AlertRepository(IConfiguration conf, InstanceContext instance, HttpClient http)
        {
            _conf = conf;

            if (instance == InstanceContext.DeployedOrLocal
                || instance == InstanceContext.End2EndTest)
            {
                var connect = new HttpClientHandler();

                connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };
                connect.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;

                _http = new HttpClient(connect);
                _http.BaseAddress = new Uri($"{_conf["AlertUrls:BaseApiUrl"]}/{_conf["AlertUrls:BaseApiPath"]}/");
            }
            else
                _http = http;

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async ValueTask<HttpResponseMessage> Dequeue_EmailV1(string jwt, Guid emailID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("dequeue/v1/email/" + emailID.ToString());
        }

        public async ValueTask<HttpResponseMessage> Dequeue_TextV1(string jwt, Guid textID)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.DeleteAsync("dequeue/v1/text/" + textID.ToString());
        }

        public async ValueTask<HttpResponseMessage> Enqueue_EmailV1(string jwt, EmailV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("enqueue/v1/email",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }

        public async ValueTask<HttpResponseMessage> Enqueue_TextV1(string jwt, TextV1 model)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);

            return await _http.PostAsync("enqueue/v1/text",
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        }
    }
}
