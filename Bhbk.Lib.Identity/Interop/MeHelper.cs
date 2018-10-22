using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Core.Primitives.Enums;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Bhbk.Lib.Identity.Interop
{
    public class MeHelper
    {
        protected readonly IConfigurationRoot _conf;
        protected readonly ContextType _context;
        protected readonly HttpClient _connect;

        public MeHelper(IConfigurationRoot conf, ContextType context)
        {
            if (conf == null)
                throw new ArgumentNullException();

            var connect = new HttpClientHandler();

            //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
            connect.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

            _context = context;
            _conf = conf;
            _connect = new HttpClient(connect);
        }

        public MeHelper(IConfigurationRoot conf, TestServer connect)
        {
            if (conf == null)
                throw new ArgumentNullException();

            _context = ContextType.UnitTest;
            _conf = conf;
            _connect = connect.CreateClient();
        }
    }
}
