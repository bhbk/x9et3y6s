using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

namespace Bhbk.Lib.Identity.Services
{
    public class AlertService : IAlertService
    {
        private readonly ResourceOwnerHelper _jwt;

        public AlertService(IConfigurationRoot conf, InstanceContext instance, HttpClient client)
        {
            _jwt = new ResourceOwnerHelper(conf, instance, client);
            HttpClient = new AlertRepository(conf, instance, client);
        }

        public JwtSecurityToken Jwt
        {
            get { return _jwt.JwtV2; }
            set { _jwt.JwtV2 = value; }
        }

        public AlertRepository HttpClient { get; }

        public bool Email_EnqueueV1(EmailCreate model)
        {
            var response = HttpClient.Enqueue_EmailV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }

        public bool Text_EnqueueV1(TextCreate model)
        {
            var response = HttpClient.Enqueue_TextV1(_jwt.JwtV2.RawData, model).Result;

            if (response.IsSuccessStatusCode)
                return true;

            throw new HttpRequestException(response.RequestMessage
                + Environment.NewLine + response);
        }
    }
}
