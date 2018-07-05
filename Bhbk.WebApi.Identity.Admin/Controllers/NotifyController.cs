using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.WebApi.Identity.Admin.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("notify")]
    public class NotifyController : BaseController
    {
        public NotifyController() { }

        public NotifyController(IConfigurationRoot conf, IIdentityContext ioc, IHostedService[] tasks)
            : base(conf, ioc, tasks) { }

        [Route("v1/email"), HttpPost]
        public async Task<IActionResult> SendEmailV1([FromBody] UserCreateEmail model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await IoC.UserMgmt.FindByIdAsync(model.ToId.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (user.Email != model.ToEmail)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var queue = ((QueueEmailTask)Tasks.Single(x => x.GetType() == typeof(QueueEmailTask)));

            if (!queue.TryEnqueueEmail(model))
                return BadRequest(BaseLib.Statics.MsgSysQueueEmailError);

            return Ok();
        }

        [Route("v1/text"), HttpPost]
        public async Task<IActionResult> SendTextV1([FromBody] UserCreateText model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await IoC.UserMgmt.FindByIdAsync(model.ToId.ToString());

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            else if (user.PhoneNumber != model.ToPhoneNumber)
                return BadRequest(BaseLib.Statics.MsgUserInvalid);

            var queue = ((QueueTextTask)Tasks.Single(x => x.GetType() == typeof(QueueTextTask)));

            if (!queue.TryEnqueueText(model))
                return BadRequest(BaseLib.Statics.MsgSysQueueSmsError);

            return Ok();
        }
    }
}
