using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Controller
{
    [RoutePrefix("claim")]
    [Authorize(Roles = "(Built-In) Administrators")]
    public class ClaimController : BaseController
    {
        public ClaimController() { }

        public ClaimController(IUnitOfWork uow)
            : base(uow) { }

        [Route("v1/{userID}"), HttpPut]
        public async Task<IHttpActionResult> CreateClaim(Guid userID, UserClaimModel.Binding.Create model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var foundUser = await UoW.CustomUserManager.FindByIdAsync(userID);

            if (foundUser == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                var newClaim = new AppUserClaim()
                {
                    UserId = foundUser.Id,
                    ClaimType = model.ClaimType,
                    ClaimValue = model.ClaimValue,
                    ClaimValueType = model.ClaimValueType,
                    Issuer = model.Issuer,
                    OriginalIssuer = model.OriginalIssuer
                };
                await UoW.CustomUserManager.AddClaimAsync(foundUser.Id, new Claim(model.ClaimType, model.ClaimValue));

                return Ok(ModelFactory.Create(newClaim));
            }
        }

        [Route("v1/{userID}"), HttpPut]
        public async Task<IHttpActionResult> DeleteClaim(Guid userID, Guid claimID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var foundUser = await UoW.CustomUserManager.FindByIdAsync(userID);

            if (foundUser == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                var foundClaim = foundUser.Claims.Where(x => x.Id == claimID).Single();

                await UoW.CustomUserManager.RemoveClaimAsync(foundUser.Id,
                    new Claim(foundClaim.ClaimType, foundClaim.ClaimValue));

                return Ok();
            }
        }

        [Route("v1"), HttpGet]
        [Authorize]
        public IHttpActionResult GetClaims()
        {
            var user = User.Identity as ClaimsIdentity;

            var claims = user.Claims.Select(x => new
            {
                x.Subject.Name,
                x.Type,
                x.Value
            });

            return Ok(claims);
        }

        [Route("v1/{userID}"), HttpGet]
        [Authorize]
        public async Task<IHttpActionResult> GetClaims(Guid userID)
        {
            var foundUser = await UoW.CustomUserManager.FindByIdAsync(userID);

            if (foundUser == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                var rslt = new List<AppUserClaim>();
                var claims = await UoW.CustomUserManager.GetClaimsAsync(userID);

                foreach (var claim in claims)
                {
                    var model = new AppUserClaim()
                    {
                        UserId = foundUser.Id,
                        ClaimType = claim.Type,
                        ClaimValue = claim.Value,
                        ClaimValueType = claim.ValueType,
                        Issuer = claim.Issuer,
                        OriginalIssuer = claim.OriginalIssuer,
                        Immutable = false
                    };

                    rslt.Add(model);
                }

                return Ok(rslt.ToList().Select(x => ModelFactory.Create(x)));
            }
        }

        [Route("v1/{userID}"), HttpPut]
        public async Task<IHttpActionResult> UpdateClaims(Guid userID, IDictionary<string, string> claims)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var foundUser = await UoW.CustomUserManager.FindByIdAsync(userID);

            if (foundUser == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                var result = await UoW.CustomUserManager.CreateIdentityAsync(foundUser, "JWT");

                foreach (var claim in result.Claims)
                    await UoW.CustomUserManager.RemoveClaimAsync(userID, claim);

                foreach (var claim in claims)
                    await UoW.CustomUserManager.AddClaimAsync(userID, new Claim(claim.Key, claim.Value));

                return Ok();
            }
        }
    }
}
