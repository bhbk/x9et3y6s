using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Hashing;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Domain.Providers.Sts;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

/*
 * https://tools.ietf.org/html/rfc6749#section-4.3
 * https://tools.ietf.org/html/rfc6749#section-6
 */

/*
 * https://oauth.net/2/grant-types/password/
 * https://oauth.net/2/grant-types/refresh-token/
 */

namespace Bhbk.WebApi.Identity.Sts.Controllers
{
    [Route("oauth2")]
    public class ResourceOwnerController : BaseController
    {
        private ResourceOwnerProvider _provider;

        public ResourceOwnerController(IConfiguration conf, IContextService instance)
        {
            _provider = new ResourceOwnerProvider(conf, instance);
        }

        [Route("v1/ropg"), HttpPost]
        [AllowAnonymous]
        public IActionResult ResourceOwnerV1_Grant([FromForm] ResourceOwnerV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //check if issuer compatibility mode enabled.
            var legacyIssuer = UoW.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == Constants.SettingGlobalLegacyIssuer).Single();

            if (!bool.Parse(legacyIssuer.ConfigValue)
                && string.IsNullOrEmpty(input.issuer_id))
            {
                ModelState.AddModelError(MessageType.IssuerInvalid.ToString(), $"Issuer:None");
                return BadRequest(ModelState);
            }

            Guid issuerID;
            tbl_Issuer issuer;

            if (bool.Parse(legacyIssuer.ConfigValue)
                && string.IsNullOrEmpty(input.issuer_id))
            {
                //really gross but needed for backward compatibility. can be lame if more than one issuer.
                if (UoW.InstanceType == InstanceContext.DeployedOrLocal
                    || UoW.InstanceType == InstanceContext.End2EndTest)
                    issuer = UoW.Issuers.Get(x => x.Name == Conf.GetSection("IdentityTenants:AllowedIssuers").GetChildren()
                        .Select(i => i.Value).First()).SingleOrDefault();

                else if (UoW.InstanceType == InstanceContext.SystemTest
                    || UoW.InstanceType == InstanceContext.IntegrationTest)
                    issuer = UoW.Issuers.Get(x => x.Name == Constants.TestIssuer).SingleOrDefault();

                else
                    throw new NotImplementedException();
            }
            else
            {
                //check if identifier is guid. resolve to guid if not.
                if (Guid.TryParse(input.issuer_id, out issuerID))
                    issuer = UoW.Issuers.Get(x => x.Id == issuerID).SingleOrDefault();
                else
                    issuer = UoW.Issuers.Get(x => x.Name == input.issuer_id).SingleOrDefault();
            }

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{input.issuer_id}");
                return NotFound(ModelState);
            }
            else if (!issuer.IsEnabled)
            {
                ModelState.AddModelError(MessageType.IssuerInvalid.ToString(), $"Issuer:{issuer.Id}");
                return BadRequest(ModelState);
            }

            Guid audienceID;
            tbl_Audience audience;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.client_id, out audienceID))
                audience = UoW.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();
            else
                audience = UoW.Audiences.Get(x => x.Name == input.client_id).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{input.client_id}");
                return NotFound(ModelState);
            }
            else if (!audience.IsEnabled)
            {
                ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Audience:{audience.Id}");
                return BadRequest(ModelState);
            }

            Guid userID;
            tbl_User user;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.username, out userID))
                user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();
            else
                user = UoW.Users.Get(x => x.UserName == input.username).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{input.username}");
                return NotFound(ModelState);
            }
            //check that user is confirmed...
            //check that user is not locked...
            else if (UoW.Users.IsLockedOut(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            var logins = UoW.Logins.Get(QueryExpressionFactory.GetQueryExpression<tbl_Login>()
                .Where(x => x.tbl_UserLogins.Any(y => y.UserId == user.Id)).ToLambda());

            switch (UoW.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                case InstanceContext.End2EndTest:
                    {
                        //check if login provider is local...
                        if (logins.Where(x => x.Name.Equals(Constants.DefaultLogin, StringComparison.OrdinalIgnoreCase)).Any())
                        {
                            //check that password is valid...
                            if (!PBKDF2.Validate(user.PasswordHashPBKDF2, input.password))
                            {
                                //adjust counter(s) for login failure...
                                UoW.Users.AccessFailed(user);
                                UoW.Commit();

                                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                                return BadRequest(ModelState);
                            }
                            else
                            {
                                //adjust counter(s) for login success...
                                UoW.Users.AccessSuccess(user);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"No login for user:{user.Id}");
                            return NotFound(ModelState);
                        }
                    }
                    break;

                case InstanceContext.SystemTest:
                case InstanceContext.IntegrationTest:
                    {
                        //check if login provider is local or test...
                        if (logins.Where(x => x.Name.Equals(Constants.DefaultLogin, StringComparison.OrdinalIgnoreCase)).Any()
                            || logins.Where(x => x.Name.StartsWith(Constants.TestLogin, StringComparison.OrdinalIgnoreCase)).Any())
                        {
                            //check that password is valid...
                            if (!PBKDF2.Validate(user.PasswordHashPBKDF2, input.password))
                            {
                                //adjust counter(s) for login failure...
                                UoW.Users.AccessFailed(user);
                                UoW.Commit();

                                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                                return BadRequest(ModelState);
                            }
                            else
                            {
                                //adjust counter(s) for login success...
                                UoW.Users.AccessSuccess(user);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"No login for user:{user.Id}");
                            return NotFound(ModelState);
                        }

                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (bool.Parse(legacyIssuer.ConfigValue)
                && string.IsNullOrEmpty(input.issuer_id))
            {
                var rop_claims = UoW.Users.GenerateAccessClaims(user);
                var rop = Auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, null, new List<string> { audience.Name }, rop_claims);

                UoW.Activities.Create(
                    Mapper.Map<tbl_Activity>(new ActivityV1()
                    {
                        UserId = user.Id,
                        ActivityType = LoginType.CreateUserAccessTokenV1Legacy.ToString(),
                        IsDeletable = false
                    }));

                UoW.Commit();

                var result = new UserJwtV1Legacy()
                {
                    token_type = "bearer",
                    access_token = rop.RawData,
                    expires_in = (int)(new DateTimeOffset(rop.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds,
                };

                return Ok(result);
            }
            else
            {
                var rop_claims = UoW.Users.GenerateAccessClaims(issuer, user);
                var rop = Auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, Conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

                UoW.Activities.Create(
                    Mapper.Map<tbl_Activity>(new ActivityV1()
                    {
                        UserId = user.Id,
                        ActivityType = LoginType.CreateUserAccessTokenV1.ToString(),
                        IsDeletable = false
                    }));

                var rt_claims = UoW.Users.GenerateRefreshClaims(issuer, user);
                var rt = Auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, Conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

                UoW.Refreshes.Create(
                    Mapper.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = RefreshType.User.ToString(),
                        RefreshValue = rt.RawData,
                        IssuedUtc = rt.ValidFrom,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                    }));

                UoW.Activities.Create(
                    Mapper.Map<tbl_Activity>(new ActivityV1()
                    {
                        UserId = user.Id,
                        ActivityType = LoginType.CreateUserRefreshTokenV1.ToString(),
                        IsDeletable = false
                    }));

                UoW.Commit();

                var result = new UserJwtV1()
                {
                    token_type = "bearer",
                    access_token = rop.RawData,
                    refresh_token = rt.RawData,
                    user_id = user.UserName,
                    client_id = audience.Name,
                    issuer_id = issuer.Name + ":" + Conf["IdentityTenants:Salt"],
                    expires_in = (int)(new DateTimeOffset(rop.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds,
                };

                return Ok(result);
            }
        }

        [Route("v1/ropg-rt"), HttpPost]
        [AllowAnonymous]
        public IActionResult ResourceOwnerV1_Refresh([FromForm] RefreshTokenV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refresh = UoW.Refreshes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.RefreshValue == input.refresh_token).ToLambda()).SingleOrDefault();

            if (refresh == null)
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{input.refresh_token}");
                return NotFound(ModelState);
            }
            else if (!string.Equals(refresh.RefreshType, RefreshType.User.ToString(), StringComparison.OrdinalIgnoreCase)
                || (refresh.ValidFromUtc >= DateTime.UtcNow || refresh.ValidToUtc <= DateTime.UtcNow))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{input.refresh_token}");
                return BadRequest(ModelState);
            }

            Guid issuerID;
            tbl_Issuer issuer;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.issuer_id, out issuerID))
                issuer = UoW.Issuers.Get(x => x.Id == issuerID).SingleOrDefault();
            else
                issuer = UoW.Issuers.Get(x => x.Name == input.issuer_id).SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{input.issuer_id}");
                return NotFound(ModelState);
            }
            else if (!issuer.IsEnabled)
            {
                ModelState.AddModelError(MessageType.IssuerInvalid.ToString(), $"Issuer:{issuer.Id}");
                return BadRequest(ModelState);
            }

            Guid audienceID;
            tbl_Audience audience;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.client_id, out audienceID))
                audience = UoW.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();
            else
                audience = UoW.Audiences.Get(x => x.Name == input.client_id).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{input.client_id}");
                return NotFound(ModelState);
            }
            else if (!audience.IsEnabled)
            {
                ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Audience:{audience.Id}");
                return BadRequest(ModelState);
            }

            var user = UoW.Users.Get(x => x.Id == refresh.UserId).SingleOrDefault();

            //check that user exists...
            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{refresh.UserId}");
                return NotFound(ModelState);
            }
            //check that user is not locked...
            else if (UoW.Users.IsLockedOut(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            var rop_claims = UoW.Users.GenerateAccessClaims(issuer, user);
            var rop = Auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, Conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rop_claims);

            var rt_claims = UoW.Users.GenerateRefreshClaims(issuer, user);
            var rt = Auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, Conf["IdentityTenants:Salt"], new List<string>() { audience.Name }, rt_claims);

            UoW.Refreshes.Create(
                Mapper.Map<tbl_Refresh>(new RefreshV1()
                {
                    IssuerId = issuer.Id,
                    UserId = user.Id,
                    RefreshType = RefreshType.User.ToString(),
                    RefreshValue = rt.RawData,
                    IssuedUtc = rt.ValidFrom,
                    ValidFromUtc = rt.ValidFrom,
                    ValidToUtc = rt.ValidTo,
                }));

            UoW.Activities.Create(
                Mapper.Map<tbl_Activity>(new ActivityV1()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserRefreshTokenV1.ToString(),
                    IsDeletable = false
                }));

            UoW.Commit();

            var result = new UserJwtV1()
            {
                token_type = "bearer",
                access_token = rop.RawData,
                refresh_token = rt.RawData,
                user_id = user.UserName,
                client_id = audience.Name,
                issuer_id = issuer.Name + ":" + Conf["IdentityTenants:Salt"],
                expires_in = (int)(new DateTimeOffset(rop.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            return Ok(result);
        }

        [Route("v2/ropg"), HttpPost]
        [AllowAnonymous]
        public IActionResult ResourceOwnerV2_Grant([FromForm] ResourceOwnerV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Guid issuerID;
            tbl_Issuer issuer;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.issuer, out issuerID))
                issuer = UoW.Issuers.Get(x => x.Id == issuerID).SingleOrDefault();
            else
                issuer = UoW.Issuers.Get(x => x.Name == input.issuer).SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{input.issuer}");
                return NotFound(ModelState);
            }
            else if (!issuer.IsEnabled)
            {
                ModelState.AddModelError(MessageType.IssuerInvalid.ToString(), $"Issuer:{issuer.Id}");
                return BadRequest(ModelState);
            }

            Guid userID;
            tbl_User user;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.user, out userID))
                user = UoW.Users.Get(x => x.Id == userID).SingleOrDefault();
            else
                user = UoW.Users.Get(x => x.UserName == input.user).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{input.user}");
                return NotFound(ModelState);
            }
            //check that user is confirmed...
            //check that user is not locked...
            else if (UoW.Users.IsLockedOut(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            var audienceList = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                    .Where(x => x.tbl_Roles.Any(y => y.tbl_UserRoles.Any(z => z.UserId == user.Id))).ToLambda());
            var audiences = new List<tbl_Audience>();

            //check if client is single, multiple or undefined...
            if (string.IsNullOrEmpty(input.client))
                audiences = UoW.Audiences.Get(x => audienceList.Contains(x)
                    && x.IsEnabled == true).ToList();
            else
            {
                foreach (string entry in input.client.Split(","))
                {
                    Guid audienceID;
                    tbl_Audience audience;

                    //check if identifier is guid. resolve to guid if not.
                    if (Guid.TryParse(entry.Trim(), out audienceID))
                        audience = UoW.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();
                    else
                        audience = UoW.Audiences.Get(x => x.Name == entry.Trim()).SingleOrDefault();

                    if (audience == null)
                    {
                        ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{entry}");
                        return NotFound(ModelState);
                    }
                    else if (!audience.IsEnabled
                        || !audienceList.Contains(audience))
                    {
                        ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Audience:{audience.Id}");
                        return BadRequest(ModelState);
                    }

                    audiences.Add(audience);
                }
            }

            if (audiences.Count() == 0)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:None");
                return BadRequest(ModelState);
            }

            var logins = UoW.Logins.Get(QueryExpressionFactory.GetQueryExpression<tbl_Login>()
                .Where(x => x.tbl_UserLogins.Any(y => y.UserId == user.Id)).ToLambda());

            switch (UoW.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                case InstanceContext.End2EndTest:
                    {
                        //check if login provider is local...
                        if (logins.Where(x => x.Name.Equals(Constants.DefaultLogin, StringComparison.OrdinalIgnoreCase)).Any())
                        {
                            //check that password is valid...
                            if (!PBKDF2.Validate(user.PasswordHashPBKDF2, input.password))
                            {
                                //adjust counter(s) for login failure...
                                UoW.Users.AccessFailed(user);
                                UoW.Commit();

                                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                                return BadRequest(ModelState);
                            }
                            else
                            {
                                //adjust counter(s) for login success...
                                UoW.Users.AccessSuccess(user);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"No login for user:{user.Id}");
                            return NotFound(ModelState);
                        }
                    }
                    break;

                case InstanceContext.SystemTest:
                case InstanceContext.IntegrationTest:
                    {
                        //check if login provider is local or test...
                        if (logins.Where(x => x.Name.Equals(Constants.DefaultLogin, StringComparison.OrdinalIgnoreCase)).Any()
                            || logins.Where(x => x.Name.StartsWith(Constants.TestLogin, StringComparison.OrdinalIgnoreCase)).Any())
                        {
                            //check that password is valid...
                            if (!PBKDF2.Validate(user.PasswordHashPBKDF2, input.password))
                            {
                                //adjust counter(s) for login failure...
                                UoW.Users.AccessFailed(user);
                                UoW.Commit();

                                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                                return BadRequest(ModelState);
                            }
                            else
                            {
                                //adjust counter(s) for login success...
                                UoW.Users.AccessSuccess(user);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(MessageType.LoginNotFound.ToString(), $"No login for user:{user.Id}");
                            return NotFound(ModelState);
                        }

                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            var rop_claims = UoW.Users.GenerateAccessClaims(issuer, user);
            var rop = Auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, Conf["IdentityTenants:Salt"], audiences.Select(x => x.Name).ToList(), rop_claims);

            UoW.Activities.Create(
                Mapper.Map<tbl_Activity>(new ActivityV1()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserAccessTokenV2.ToString(),
                    IsDeletable = false
                }));

            var rt_claims = UoW.Users.GenerateRefreshClaims(issuer, user);
            var rt = Auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, Conf["IdentityTenants:Salt"], audiences.Select(x => x.Name).ToList(), rt_claims);

            UoW.Refreshes.Create(
                Mapper.Map<tbl_Refresh>(new RefreshV1()
                {
                    IssuerId = issuer.Id,
                    UserId = user.Id,
                    RefreshType = RefreshType.User.ToString(),
                    RefreshValue = rt.RawData,
                    IssuedUtc = rt.ValidFrom,
                    ValidFromUtc = rt.ValidFrom,
                    ValidToUtc = rt.ValidTo,
                }));

            UoW.Activities.Create(
                Mapper.Map<tbl_Activity>(new ActivityV1()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserRefreshTokenV2.ToString(),
                    IsDeletable = false
                }));

            UoW.Commit();

            var result = new UserJwtV2()
            {
                token_type = "bearer",
                access_token = rop.RawData,
                refresh_token = rt.RawData,
                user = user.UserName,
                client = audiences.Select(x => x.Name).ToList(),
                issuer = issuer.Name + ":" + Conf["IdentityTenants:Salt"],
                expires_in = (int)(new DateTimeOffset(rop.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            return Ok(result);
        }

        [Route("v2/ropg-rt"), HttpPost]
        [AllowAnonymous]
        public IActionResult ResourceOwnerV2_Refresh([FromForm] RefreshTokenV2 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refresh = UoW.Refreshes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.RefreshValue == input.refresh_token).ToLambda()).SingleOrDefault();

            if (refresh == null)
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{input.refresh_token}");
                return NotFound(ModelState);
            }
            else if (!string.Equals(refresh.RefreshType, RefreshType.User.ToString(), StringComparison.OrdinalIgnoreCase)
                || (refresh.ValidFromUtc >= DateTime.UtcNow || refresh.ValidToUtc <= DateTime.UtcNow))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{input.refresh_token}");
                return BadRequest(ModelState);
            }

            Guid issuerID;
            tbl_Issuer issuer;

            //check if identifier is guid. resolve to guid if not.
            if (Guid.TryParse(input.issuer, out issuerID))
                issuer = UoW.Issuers.Get(x => x.Id == issuerID).SingleOrDefault();
            else
                issuer = UoW.Issuers.Get(x => x.Name == input.issuer).SingleOrDefault();

            if (issuer == null)
            {
                ModelState.AddModelError(MessageType.IssuerNotFound.ToString(), $"Issuer:{input.issuer}");
                return NotFound(ModelState);
            }
            else if (!issuer.IsEnabled)
            {
                ModelState.AddModelError(MessageType.IssuerInvalid.ToString(), $"Issuer:{issuer.Id}");
                return BadRequest(ModelState);
            }

            var user = UoW.Users.Get(x => x.Id == refresh.UserId).SingleOrDefault();

            //check that user exists...
            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{refresh.UserId}");
                return NotFound(ModelState);
            }
            //check that user is not locked...
            else if (UoW.Users.IsLockedOut(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            //no context for auth exists yet... so set actor id same as user id...
            user.ActorId = user.Id;

            var clientList = UoW.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                    .Where(x => x.tbl_Roles.Any(y => y.tbl_UserRoles.Any(z => z.UserId == user.Id))).ToLambda());
            var audiences = new List<tbl_Audience>();

            //check if client is single, multiple or undefined...
            if (string.IsNullOrEmpty(input.client))
                audiences = UoW.Audiences.Get(x => clientList.Contains(x)
                    && x.IsEnabled == true).ToList();
            else
            {
                foreach (string entry in input.client.Split(","))
                {
                    Guid audienceID;
                    tbl_Audience audience;

                    //check if identifier is guid. resolve to guid if not.
                    if (Guid.TryParse(entry.Trim(), out audienceID))
                        audience = UoW.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();
                    else
                        audience = UoW.Audiences.Get(x => x.Name == entry.Trim()).SingleOrDefault();

                    if (audience == null)
                    {
                        ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{entry}");
                        return NotFound(ModelState);
                    }
                    else if (!audience.IsEnabled
                        || !clientList.Contains(audience))
                    {
                        ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Audience:{audience.Id}");
                        return BadRequest(ModelState);
                    }

                    audiences.Add(audience);
                }
            }

            if (audiences.Count() == 0)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:None");
                return BadRequest(ModelState);
            }

            var rop_claims = UoW.Users.GenerateAccessClaims(issuer, user);
            var rop = Auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, Conf["IdentityTenants:Salt"], audiences.Select(x => x.Name).ToList(), rop_claims);

            var rt_claims = UoW.Users.GenerateRefreshClaims(issuer, user);
            var rt = Auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, Conf["IdentityTenants:Salt"], audiences.Select(x => x.Name).ToList(), rt_claims);

            UoW.Refreshes.Create(
                Mapper.Map<tbl_Refresh>(new RefreshV1()
                {
                    IssuerId = issuer.Id,
                    UserId = user.Id,
                    RefreshType = RefreshType.User.ToString(),
                    RefreshValue = rt.RawData,
                    IssuedUtc = rt.ValidFrom,
                    ValidFromUtc = rt.ValidFrom,
                    ValidToUtc = rt.ValidTo,
                }));

            UoW.Activities.Create(
                Mapper.Map<tbl_Activity>(new ActivityV1()
                {
                    UserId = user.Id,
                    ActivityType = LoginType.CreateUserRefreshTokenV2.ToString(),
                    IsDeletable = false
                }));

            UoW.Commit();

            var result = new UserJwtV2()
            {
                token_type = "bearer",
                access_token = rop.RawData,
                refresh_token = rt.RawData,
                user = user.UserName,
                client = audiences.Select(x => x.Name).ToList(),
                issuer = issuer.Name + ":" + Conf["IdentityTenants:Salt"],
                expires_in = (int)(new DateTimeOffset(rop.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            return Ok(result);
        }
    }
}
