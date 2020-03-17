using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Domain.Providers.Alert;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.WebApi.Alert.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Bhbk.WebApi.Alert.Controllers
{
    [Route("email")]
    public class EmailController : BaseController
    {
        private EmailProvider _provider;

        public EmailController(IConfiguration conf, IContextService instance)
        {
            _provider = new EmailProvider(conf, instance);
        }

        [Route("v1"), HttpPost]
        public IActionResult SendEmailV1([FromBody] EmailV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!UoW.Users.Get(x => x.Id == model.FromId
                && x.EmailAddress == model.FromEmail).Any())
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"SenderID:{model.FromId} SenderEmail:{model.FromEmail}");
                return NotFound(ModelState);
            }

            var queue = (QueueEmailTask)Tasks.Single(x => x.GetType() == typeof(QueueEmailTask));

            if (!queue.TryEnqueueEmail(model))
            {
                ModelState.AddModelError(MessageType.EmailEnueueError.ToString(), $"SenderID:{model.FromId} SenderEmail:{model.FromEmail}");
                return BadRequest(ModelState);
            }

            return NoContent();
        }
    }
}
