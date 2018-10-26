using Bhbk.Lib.Core.Models;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("activity")]
    public class ActivityController : BaseController
    {
        public ActivityController() { }

        public ActivityController(IConfigurationRoot conf, IIdentityContext<AppDbContext> uow, IHostedService[] tasks)
            : base(conf, uow, tasks) { }

        [Route("v1"), HttpGet]
        [Authorize(Roles = "(Built-In) Administrators")]
        public async Task<IActionResult> GetActivityV1([FromQuery] Paging model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var activity = await UoW.ActivityRepo.GetAsync(x => true, 
                x => x.OrderBy(model.OrderBy).Skip(model.Skip).Take(model.Take));

            var result = activity.Select(x => UoW.Maps.Map<AppActivity>(x));

            return Ok(result);
        }
    }
}