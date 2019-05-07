using Bhbk.Lib.Identity.Data.Models;
using Bhbk.Lib.Identity.Data.Primitives.Enums;
using Bhbk.Lib.Identity.Models.Alert;
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
    //[Authorize(Policy = "UsersPolicy")]
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
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"SenderID:{model.FromId} SenderEmail:{model.FromEmail}");
                return NotFound(ModelState);
            }

            var queue = (QueueEmailTask)Tasks.Single(x => x.GetType() == typeof(QueueEmailTask));

            var msg = UoW.Mapper.Map<tbl_QueueEmails>(model);

            if (!queue.TryEnqueueEmail(msg))
            {
                ModelState.AddModelError(MessageType.EmailEnueueError.ToString(), $"MessageID:{msg.Id} SenderEmail:{model.FromEmail}");
                return BadRequest(ModelState);
            }

            return NoContent();
        }
    }
}
