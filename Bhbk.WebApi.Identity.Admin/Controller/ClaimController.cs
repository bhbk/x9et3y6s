﻿using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interface;
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
        public async Task<IHttpActionResult> CreateClaim(Guid userID, UserClaimModel.Create model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgmt.FindByIdAsyncDeprecated(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                var claim = UoW.Models.Create.DoIt(model);
                await UoW.UserMgmt.AddClaimAsync(user.Id, new Claim(claim.ClaimType, claim.ClaimValue));

                return Ok(claim);
            }
        }

        [Route("v1/{userID}"), HttpPut]
        public async Task<IHttpActionResult> DeleteClaim(Guid userID, Guid claimID)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgmt.FindByIdAsyncDeprecated(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                var claim = user.Claims.Where(x => x.Id == claimID).Single();

                await UoW.UserMgmt.RemoveClaimAsync(user.Id,
                    new Claim(claim.ClaimType, claim.ClaimValue));

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
            var user = await UoW.UserMgmt.FindByIdAsyncDeprecated(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                IList<UserClaimModel.Model> rslt = new List<UserClaimModel.Model>();
                var claims = await UoW.UserMgmt.GetClaimsAsync(userID);

                foreach (var claim in claims)
                {
                    var model = new UserClaimModel.Model()
                    {
                        UserId = user.Id,
                        ClaimType = claim.Type,
                        ClaimValue = claim.Value,
                        ClaimValueType = claim.ValueType,
                        Issuer = claim.Issuer,
                        OriginalIssuer = claim.OriginalIssuer,
                        Created = DateTime.Now,
                        Immutable = false
                    };

                    rslt.Add(model);
                }

                return Ok(rslt);
            }
        }

        [Route("v1/{userID}"), HttpPut]
        public async Task<IHttpActionResult> UpdateClaims(Guid userID, IDictionary<string, string> claims)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UoW.UserMgmt.FindByIdAsyncDeprecated(userID);

            if (user == null)
                return BadRequest(BaseLib.Statics.MsgUserNotExist);

            else
            {
                var result = await UoW.UserMgmt.CreateIdentityAsync(user, "JWT");

                foreach (var claim in result.Claims)
                    await UoW.UserMgmt.RemoveClaimAsync(userID, claim);

                foreach (var claim in claims)
                    await UoW.UserMgmt.AddClaimAsync(userID, new Claim(claim.Key, claim.Value));

                return Ok();
            }
        }
    }
}
