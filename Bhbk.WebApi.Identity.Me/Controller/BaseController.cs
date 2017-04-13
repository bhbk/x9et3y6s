using Bhbk.Lib.Identity.Infrastructure;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Net.Http;
using System.Web.Http;

namespace Bhbk.WebApi.Identity.Me.Controller
{
    //[Authorize]
    [AllowAnonymous]
    public class BaseController : ApiController
    {
        private CustomModelFactory _mf = null;
        private IUnitOfWork _uow = null;
        protected IUnitOfWork UoW
        {
            get
            {
                return _uow ?? Request.GetOwinContext().GetUserManager<IUnitOfWork>();
            }
        }
        protected CustomModelFactory ModelFactory
        {
            get
            {
                return _mf ?? new CustomModelFactory(Request.GetOwinContext().GetUserManager<IUnitOfWork>());
            }
        }

        public BaseController() { }

        public BaseController(IUnitOfWork uow)
        {
            if (uow == null)
                throw new ArgumentNullException();

            this._uow = uow;
            this._mf = new CustomModelFactory(this._uow);
        }

        protected IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
                return InternalServerError();

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                        ModelState.AddModelError("", error);
                }

                if (ModelState.IsValid)
                    return BadRequest();

                return BadRequest(ModelState);
            }

            return null;
        }
    }
}
