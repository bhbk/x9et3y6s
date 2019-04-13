using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Internal.Primitives.Enums;
using Bhbk.WebApi.Alert.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Alert.Controllers
{
    [Route("email")]
    public class EmailController : BaseController
    {
        public EmailController() { }

        [Route("v1"), HttpPost]
        public async Task<IActionResult> SendEmailV1([FromBody] EmailCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!(await UoW.UserRepo.GetAsync(x => x.Id == model.FromId
                && x.Email == model.FromEmail)).Any())
            {
                ModelState.AddModelError(MsgType.UserNotFound.ToString(), $"SenderID:{model.FromId} SenderEmail:{model.FromEmail}");
                return NotFound(ModelState);
            }

            var queue = ((QueueEmailTask)Tasks.Single(x => x.GetType() == typeof(QueueEmailTask)));

            if (!queue.TryEnqueueEmail(model))
            {
                ModelState.AddModelError(MsgType.EmailEnueueError.ToString(), $"MessageID:{model.Id} SenderEmail:{model.FromEmail}");
                return BadRequest(ModelState);
            }

            return NoContent();
        }
    }
}
