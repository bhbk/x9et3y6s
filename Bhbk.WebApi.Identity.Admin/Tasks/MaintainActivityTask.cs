using Bhbk.Lib.Identity.Helpers;
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
        private readonly IIdentityContext _ioc;
        private readonly IConfigurationRoot _cb;
        private readonly JsonSerializerSettings _serializer;
        private readonly FileInfo _cf = FileSystemHelper.SearchPaths("appsettings-api.json");
        private readonly int _delay, _transient, _auditable;
        public string Status { get; private set; }

        public MaintainActivityTask(IIdentityContext ioc)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            _cb = new ConfigurationBuilder()
                .SetBasePath(_cf.DirectoryName)
                .AddJsonFile(_cf.Name, optional: false, reloadOnChange: true)
                .Build();

            _delay = int.Parse(_cb["Tasks:MaintainActivity:PollingDelay"]);
            _auditable = int.Parse(_cb["Tasks:MaintainActivity:HoldAuditable"]);
            _transient = int.Parse(_cb["Tasks:MaintainActivity:HoldTransient"]);
            _ioc = ioc;

            var statusMsg = typeof(MaintainActivityTask).Name + " not run yet.";

            Status = JsonConvert.SerializeObject(new
            {
                status = statusMsg
            }, _serializer);
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(_delay), cancellationToken);

                    var expired = _ioc.UserMgmt.Store.Context.AppActivity
                        .Where(x => (x.Created.AddMinutes(_transient) < DateTime.Now && x.Immutable == false)
                            || (x.Created.AddMinutes(_auditable) < DateTime.Now && x.Immutable == true));

                    var expiredCount = expired.Count();

                    if (expired.Any())
                    {
                        foreach (AppActivity entry in expired.ToList())
                            _ioc.UserMgmt.Store.Context.AppActivity.Remove(entry);

                        _ioc.UserMgmt.Store.Context.SaveChanges();

                        var statusMsg = typeof(MaintainActivityTask).Name + " success on " + DateTime.Now.ToString() + ". Delete "
                                + expiredCount.ToString() + " expired activity entries.";

                        Status = JsonConvert.SerializeObject(new
                        {
                            status = statusMsg
                        }, _serializer);

                        Log.Information(statusMsg);
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
