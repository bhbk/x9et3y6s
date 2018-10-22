using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Sts.Tasks
{
    public class MaintainTokensTask : BackgroundService
    {
        private readonly FileInfo _api = SearchRoots.ByAssemblyContext("appsettings-api.json");
        private readonly IConfigurationRoot _conf;
        private readonly IIdentityContext _ioc;
        private readonly JsonSerializerSettings _serializer;
        private readonly int _delay;
        public string Status { get; private set; }

        public MaintainTokensTask(IIdentityContext ioc)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            _conf = new ConfigurationBuilder()
                .SetBasePath(_api.DirectoryName)
                .AddJsonFile(_api.Name, optional: false, reloadOnChange: true)
                .Build();

            _delay = int.Parse(_conf["Tasks:MaintainTokens:PollingDelay"]);
            _ioc = ioc;

            Status = JsonConvert.SerializeObject(
                new
                {
                    status = typeof(MaintainTokensTask).Name + " not run yet."
                }, _serializer);
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_delay), cancellationToken);

                    var invalid = _ioc.UserMgmt.Store.Context.AppUserRefresh
                        .Where(x => x.IssuedUtc > DateTime.UtcNow || x.ExpiresUtc < DateTime.UtcNow);
                    var invalidCount = invalid.Count();

                    if (invalid.Any())
                    {
                        foreach (AppUserRefresh entry in invalid.ToList())
                            _ioc.UserMgmt.Store.Context.AppUserRefresh.Remove(entry);

                        _ioc.UserMgmt.Store.Context.SaveChanges();

                        var msg = typeof(MaintainTokensTask).Name + " success on " + DateTime.Now.ToString() + ". Delete "
                                + invalidCount.ToString() + " invalid refresh tokens.";

                        Status = JsonConvert.SerializeObject(
                            new
                            {
                                status = msg
                            }, _serializer);

                        Log.Information(msg);
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex.ToString());
                }
            }
        }
    }
}
