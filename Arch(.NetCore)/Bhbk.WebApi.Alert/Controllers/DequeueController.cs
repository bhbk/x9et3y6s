using Bhbk.Lib.Identity.Primitives.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Bhbk.WebApi.Alert.Controllers
{
    [Route("dequeue")]
    public class DequeueController : BaseController
    {
        [Route("v1/email/{emailID:guid}"), HttpDelete]
        public IActionResult DequeueEmailV1([FromRoute] Guid emailID)
        {
            var email = UoW.EmailQueue.Get(x => x.Id == emailID)
                .SingleOrDefault();

            if (email == null)
            {
                ModelState.AddModelError(MessageType.EmailNotFound.ToString(), $"Email:{emailID}");
                return NotFound(ModelState);
            }
            else
                UoW.EmailQueue.Delete(email);

            return NoContent();
        }

        [Route("v1/text/{textID:guid}"), HttpDelete]
        public IActionResult DequeueTextV1([FromRoute] Guid textID)
        {
            var text = UoW.TextQueue.Get(x => x.Id == textID)
                .SingleOrDefault();

            if (text == null)
            {
                ModelState.AddModelError(MessageType.TextNotFound.ToString(), $"Text:{textID}");
                return NotFound(ModelState);
            }
            else
                UoW.TextQueue.Delete(text);

            return NoContent();
        }
    }
}
