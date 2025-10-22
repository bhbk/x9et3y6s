using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Primitives.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Bhbk.WebApi.Identity.User.Controllers
{
    [Route("profiles")]
    [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
    public class ProfileController : BaseController
    {
        [Route("v1"), HttpGet]
        public IActionResult GetV1()
        {
            var user = uow.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            return Ok(map.Map<UserV1>(user));
        }

        [Route("v1"), HttpPut]
        public IActionResult UpdateV1([FromBody] UserV1 model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = uow.Users.Get(x => x.Id == GetIdentityGUID()).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{GetIdentityGUID()}");
                return NotFound(ModelState);
            }

            if (user.Id != model.Id
                || !user.IsHumanBeing)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            uow.Commit();

            return Ok(map.Map<UserV1>(user));
        }
    }
}
