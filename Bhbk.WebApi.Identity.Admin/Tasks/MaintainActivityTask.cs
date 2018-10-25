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

namespace Bhbk.WebApi.Identity.Admin.Tasks
{
    public class MaintainActivityTask : BackgroundService
    {
        private readonly FileInfo _api = SearchRoots.ByAssemblyContext("appsettings-api.json");
        private readonly IConfigurationRoot _conf;
        private readonly IIdentityContext<AppDbContext> _uow;
        private readonly JsonSerializerSettings _serializer;
        private readonly int _delay, _transient, _auditable;
        public string Status { get; private set; }

        public MaintainActivityTask(IIdentityContext<AppDbContext> uow)
        {
            if (uow == null)
                throw new ArgumentNullException();

            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            _conf = new ConfigurationBuilder()
                .SetBasePath(_api.DirectoryName)
                .AddJsonFile(_api.Name, optional: false, reloadOnChange: true)
                .Build();

            _delay = int.Parse(_conf["Tasks:MaintainActivity:PollingDelay"]);
            _auditable = int.Parse(_conf["Tasks:MaintainActivity:HoldAuditable"]);
            _transient = int.Parse(_conf["Tasks:MaintainActivity:HoldTransient"]);
            _uow = uow;

            Status = JsonConvert.SerializeObject(
                new
                {
                    status = typeof(MaintainActivityTask).Name + " not run yet."
                }, _serializer);
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_delay), cancellationToken);

                    var expired = _uow.CustomUserMgr.Store.Context.AppActivity
                        .Where(x => (x.Created.AddSeconds(_transient) < DateTime.Now && x.Immutable == false)
                            || (x.Created.AddSeconds(_auditable) < DateTime.Now && x.Immutable == true));

                    var expiredCount = expired.Count();

                    if (expired.Any())
                    {
                        foreach (AppActivity entry in expired.ToList())
                            _uow.CustomUserMgr.Store.Context.AppActivity.Remove(entry);

                        _uow.CustomUserMgr.Store.Context.SaveChanges();

                        var msg = typeof(MaintainActivityTask).Name + " success on " + DateTime.Now.ToString() + ". Delete "
                                + expiredCount.ToString() + " expired activity entries.";

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
                    Log.Error(ex.ToString());
                }
            }
        }
    }
}
