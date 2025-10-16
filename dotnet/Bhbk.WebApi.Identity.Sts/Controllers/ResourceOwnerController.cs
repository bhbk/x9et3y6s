using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Hashing;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
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
        [Route("v1/ropg"), HttpPost]
        [AllowAnonymous]
        public IActionResult ResourceOwnerV1_Grant([FromForm] ResourceOwnerV1 input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /* check if issuer compatibility mode enabled */
            var legacyIssuer = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                && x.ConfigKey == SettingsConstants.GlobalLegacyIssuer).Single();

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
                /* backward compatibility - can be problematic if more than one issuer */
                if (uow.InstanceType == InstanceContext.DeployedOrLocal
                    || uow.InstanceType == InstanceContext.End2EndTest)
                    issuer = uow.Issuers.Get(x => x.Name == conf.GetSection("IdentityTenant:AllowedIssuers").GetChildren()
                        .Select(i => i.Value).First()).SingleOrDefault();
                else
                    issuer = uow.Issuers.Get(x => x.Name == conf["IdentityProvider:TestIssuerName"]).SingleOrDefault();
            }
            else
            {
                /* resolve identifier to guid */
                if (Guid.TryParse(input.issuer_id, out issuerID))
                    issuer = uow.Issuers.Get(x => x.Id == issuerID).SingleOrDefault();
                else
                    issuer = uow.Issuers.Get(x => x.Name == input.issuer_id).SingleOrDefault();
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

            /* resolve identifier to guid */
            if (Guid.TryParse(input.client_id, out audienceID))
                audience = uow.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();
            else
                audience = uow.Audiences.Get(x => x.Name == input.client_id).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{input.client_id}");
                return NotFound(ModelState);
            }
            else if (audience.IsLockedOut)
            {
                ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Audience:{audience.Id}");
                return BadRequest(ModelState);
            }

            Guid userID;
            tbl_User user;

            /* resolve identifier to guid */
            if (Guid.TryParse(input.username, out userID))
                user = uow.Users.Get(x => x.Id == userID).SingleOrDefault();
            else
                user = uow.Users.Get(x => x.UserName == input.username).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{input.username}");
                return NotFound(ModelState);
            }
            /* verify user is confirmed and not locked */
            else if (uow.Users.IsLockedOut(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            var loginProviders = uow.LoginProviders.Get(QueryExpressionFactory.GetQueryExpression<tbl_LoginProvider>()
                .Where(x => x.tbl_UserLoginProviders.Any(y => y.UserId == user.Id)).ToLambda());

            switch (uow.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                case InstanceContext.End2EndTest:
                    {
                        /* check if login provider is local */
                        if (loginProviders.Where(x => x.Name.Equals(conf["IdentityProvider:BuiltInLoginProviderName"], StringComparison.OrdinalIgnoreCase)).Any())
                        {
                            /* validate password */
                            if (!PBKDF2.Validate(user.PasswordHashPBKDF2, input.password))
                            {
                                var activity = uow.AuthActivity.Post(
                                    map.Map<tbl_AuthActivity>(new AuthActivityV1()
                                    {
                                        UserId = user.Id,
                                        LoginType = GrantFlowType.ResourceOwnerPasswordV1.ToString(),
                                        LoginOutcome = GrantFlowResultType.Failure.ToString(),
                                        LocalEndpoint = Request.HttpContext.Connection.LocalIpAddress?.ToString() + ":" + Request.HttpContext.Connection.LocalPort,
                                        RemoteEndpoint = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                                    }));

                                uow.AuthActivityAudiences.Post(new tbl_AuthActivityAudience
                                {
                                    AuthActivityId = activity.Id,
                                    AudienceId = audience.Id,
                                    CreatedUtc = activity.CreatedUtc,
                                });

                                uow.Commit();

                                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                                return BadRequest(ModelState);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(MessageType.LoginProviderNotFound.ToString(), $"No login provider for user:{user.Id}");
                            return NotFound(ModelState);
                        }
                    }
                    break;

                case InstanceContext.SystemTest:
                case InstanceContext.IntegrationTest:
                    {
                        /* check if login provider is local or test */
                        if (loginProviders.Where(x => x.Name.Equals(conf["IdentityProvider:BuiltInLoginProviderName"], StringComparison.OrdinalIgnoreCase)).Any()
                            || loginProviders.Where(x => x.Name.StartsWith(conf["IdentityProvider:TestLoginProviderNamePrefix"], StringComparison.OrdinalIgnoreCase)).Any())
                        {
                            /* validate password */
                            if (!PBKDF2.Validate(user.PasswordHashPBKDF2, input.password))
                            {
                                var activity = uow.AuthActivity.Post(
                                    map.Map<tbl_AuthActivity>(new AuthActivityV1()
                                    {
                                        UserId = user.Id,
                                        LoginType = GrantFlowType.ResourceOwnerPasswordV1.ToString(),
                                        LoginOutcome = GrantFlowResultType.Failure.ToString(),
                                        LocalEndpoint = Request.HttpContext.Connection.LocalIpAddress?.ToString() + ":" + Request.HttpContext.Connection.LocalPort,
                                        RemoteEndpoint = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                                    }));

                                uow.AuthActivityAudiences.Post(new tbl_AuthActivityAudience
                                {
                                    AuthActivityId = activity.Id,
                                    AudienceId = audience.Id,
                                    CreatedUtc = activity.CreatedUtc,
                                });

                                uow.Commit();

                                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                                return BadRequest(ModelState);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(MessageType.LoginProviderNotFound.ToString(), $"No login provider for user:{user.Id}");
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
                var rop_claims = uow.Users.GenerateAccessClaims(user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, null, new List<string> { audience.Name }, rop_claims);

                var activity = uow.AuthActivity.Post(
                    map.Map<tbl_AuthActivity>(new AuthActivityV1()
                    {
                        UserId = user.Id,
                        LoginType = GrantFlowType.ResourceOwnerPasswordV1_Legacy.ToString(),
                        LoginOutcome = GrantFlowResultType.Success.ToString(),
                        LocalEndpoint = Request.HttpContext.Connection.LocalIpAddress?.ToString() + ":" + Request.HttpContext.Connection.LocalPort,
                        RemoteEndpoint = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    }));

                uow.AuthActivityAudiences.Post(new tbl_AuthActivityAudience
                {
                    AuthActivityId = activity.Id,
                    AudienceId = audience.Id,
                    CreatedUtc = activity.CreatedUtc,
                });

                uow.Commit();

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
                var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
                var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

                var acActivity = uow.AuthActivity.Post(
                    map.Map<tbl_AuthActivity>(new AuthActivityV1()
                    {
                        UserId = user.Id,
                        LoginType = GrantFlowType.ResourceOwnerPasswordV1.ToString(),
                        LoginOutcome = GrantFlowResultType.Success.ToString(),
                        LocalEndpoint = Request.HttpContext.Connection.LocalIpAddress?.ToString() + ":" + Request.HttpContext.Connection.LocalPort,
                        RemoteEndpoint = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    }));

                uow.AuthActivityAudiences.Post(new tbl_AuthActivityAudience
                {
                    AuthActivityId = acActivity.Id,
                    AudienceId = audience.Id,
                    CreatedUtc = acActivity.CreatedUtc,
                });

                var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
                var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rt_claims);

                uow.Refreshes.Post(
                    map.Map<tbl_Refresh>(new RefreshV1()
                    {
                        IssuerId = issuer.Id,
                        UserId = user.Id,
                        RefreshType = ConsumerType.User.ToString(),
                        RefreshValue = rt.RawData,
                        IssuedUtc = rt.ValidFrom,
                        ValidFromUtc = rt.ValidFrom,
                        ValidToUtc = rt.ValidTo,
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = Request.Headers["User-Agent"].ToString(),
                    }));

                var rtActivity = uow.AuthActivity.Post(
                    map.Map<tbl_AuthActivity>(new AuthActivityV1()
                    {
                        UserId = user.Id,
                        LoginType = GrantFlowType.RefreshTokenV1.ToString(),
                        LoginOutcome = GrantFlowResultType.Success.ToString(),
                        LocalEndpoint = Request.HttpContext.Connection.LocalIpAddress?.ToString() + ":" + Request.HttpContext.Connection.LocalPort,
                        RemoteEndpoint = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    }));

                uow.AuthActivityAudiences.Post(new tbl_AuthActivityAudience
                {
                    AuthActivityId = rtActivity.Id,
                    AudienceId = audience.Id,
                    CreatedUtc = rtActivity.CreatedUtc,
                });

                uow.Commit();

                SetRefreshTokenCookie(rt.RawData, rt.ValidTo);

                var result = new UserJwtV1()
                {
                    token_type = "bearer",
                    access_token = rop.RawData,
                    user_id = user.UserName,
                    client_id = audience.Name,
                    issuer_id = issuer.Name + ":" + conf["IdentityTenant:Salt"],
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

            var refreshToken = Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), "Token:Missing");
                return BadRequest(ModelState);
            }

            var refresh = uow.Refreshes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.RefreshValue == refreshToken).ToLambda()).SingleOrDefault();

            if (refresh == null)
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{refreshToken}");
                return NotFound(ModelState);
            }
            else if (!string.Equals(refresh.RefreshType, ConsumerType.User.ToString(), StringComparison.OrdinalIgnoreCase)
                || (refresh.ValidFromUtc >= DateTime.UtcNow || refresh.ValidToUtc <= DateTime.UtcNow))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{refreshToken}");
                return BadRequest(ModelState);
            }

            Guid issuerID;
            tbl_Issuer issuer;

            /* resolve identifier to guid */
            if (Guid.TryParse(input.issuer_id, out issuerID))
                issuer = uow.Issuers.Get(x => x.Id == issuerID).SingleOrDefault();
            else
                issuer = uow.Issuers.Get(x => x.Name == input.issuer_id).SingleOrDefault();

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

            /* resolve identifier to guid */
            if (Guid.TryParse(input.client_id, out audienceID))
                audience = uow.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();
            else
                audience = uow.Audiences.Get(x => x.Name == input.client_id).SingleOrDefault();

            if (audience == null)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{input.client_id}");
                return NotFound(ModelState);
            }
            else if (audience.IsLockedOut)
            {
                ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Audience:{audience.Id}");
                return BadRequest(ModelState);
            }

            var user = uow.Users.Get(x => x.Id == refresh.UserId).SingleOrDefault();

            /* check that user exists */
            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{refresh.UserId}");
                return NotFound(ModelState);
            }
            /* check that user is not locked */
            else if (uow.Users.IsLockedOut(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
            var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rop_claims);

            var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
            var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], new List<string>() { audience.Name }, rt_claims);

            uow.Refreshes.Post(
                map.Map<tbl_Refresh>(new RefreshV1()
                {
                    IssuerId = issuer.Id,
                    UserId = user.Id,
                    RefreshType = ConsumerType.User.ToString(),
                    RefreshValue = rt.RawData,
                    IssuedUtc = rt.ValidFrom,
                    ValidFromUtc = rt.ValidFrom,
                    ValidToUtc = rt.ValidTo,
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                }));

            var activity = uow.AuthActivity.Post(
                map.Map<tbl_AuthActivity>(new AuthActivityV1()
                {
                    UserId = user.Id,
                    LoginType = GrantFlowType.RefreshTokenV1.ToString(),
                    LoginOutcome = GrantFlowResultType.Success.ToString(),
                    LocalEndpoint = Request.HttpContext.Connection.LocalIpAddress?.ToString() + ":" + Request.HttpContext.Connection.LocalPort,
                    RemoteEndpoint = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                }));

            uow.AuthActivityAudiences.Post(new tbl_AuthActivityAudience
            {
                AuthActivityId = activity.Id,
                AudienceId = audience.Id,
                CreatedUtc = activity.CreatedUtc,
            });

            uow.Commit();

            SetRefreshTokenCookie(rt.RawData, rt.ValidTo);

            var result = new UserJwtV1()
            {
                token_type = "bearer",
                access_token = rop.RawData,
                user_id = user.UserName,
                client_id = audience.Name,
                issuer_id = issuer.Name + ":" + conf["IdentityTenant:Salt"],
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

            /* resolve identifier to guid */
            if (Guid.TryParse(input.issuer, out issuerID))
                issuer = uow.Issuers.Get(x => x.Id == issuerID).SingleOrDefault();
            else
                issuer = uow.Issuers.Get(x => x.Name == input.issuer).SingleOrDefault();

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

            /* resolve identifier to guid */
            if (Guid.TryParse(input.user, out userID))
                user = uow.Users.Get(x => x.Id == userID).SingleOrDefault();
            else
                user = uow.Users.Get(x => x.UserName == input.user).SingleOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{input.user}");
                return NotFound(ModelState);
            }
            /* verify user is confirmed and not locked */
            else if (uow.Users.IsLockedOut(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            var audienceList = uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                    .Where(x => x.tbl_Roles.Any(y => y.tbl_UserRoles.Any(z => z.UserId == user.Id))).ToLambda());

            var audiences = new List<tbl_Audience>();

            /* check if client is single, multiple or undefined */
            if (string.IsNullOrEmpty(input.client))
                audiences = uow.Audiences.Get(x => audienceList.Contains(x)
                    && x.IsLockedOut == false).ToList();
            else
            {
                foreach (string entry in input.client.Split(","))
                {
                    Guid audienceID;
                    tbl_Audience audience;

                    /* resolve identifier to guid */
                    if (Guid.TryParse(entry.Trim(), out audienceID))
                        audience = uow.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();
                    else
                        audience = uow.Audiences.Get(x => x.Name == entry.Trim()).SingleOrDefault();

                    if (audience == null)
                    {
                        ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{entry}");
                        return NotFound(ModelState);
                    }
                    else if (audience.IsLockedOut
                        || !audienceList.Contains(audience))
                    {
                        ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Audience:{audience.Id}");
                        return BadRequest(ModelState);
                    }

                    audiences.Add(audience);
                }
            }

            if (audiences.Count == 0)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:None");
                return BadRequest(ModelState);
            }

            var loginProviders = uow.LoginProviders.Get(QueryExpressionFactory.GetQueryExpression<tbl_LoginProvider>()
                .Where(x => x.tbl_UserLoginProviders.Any(y => y.UserId == user.Id)).ToLambda());

            switch (uow.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                case InstanceContext.End2EndTest:
                    {
                        /* check if login provider is local */
                        if (loginProviders.Where(x => x.Name.Equals(conf["IdentityProvider:BuiltInLoginProviderName"], StringComparison.OrdinalIgnoreCase)).Any())
                        {
                            /* validate password */
                            if (!PBKDF2.Validate(user.PasswordHashPBKDF2, input.password))
                            {
                                var activity = uow.AuthActivity.Post(
                                    map.Map<tbl_AuthActivity>(new AuthActivityV1()
                                    {
                                        UserId = user.Id,
                                        LoginType = GrantFlowType.ResourceOwnerPasswordV2.ToString(),
                                        LoginOutcome = GrantFlowResultType.Failure.ToString(),
                                        LocalEndpoint = Request.HttpContext.Connection.LocalIpAddress?.ToString() + ":" + Request.HttpContext.Connection.LocalPort,
                                        RemoteEndpoint = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                                    }));

                                foreach (var aud in audiences)
                                {
                                    uow.AuthActivityAudiences.Post(new tbl_AuthActivityAudience
                                    {
                                        AuthActivityId = activity.Id,
                                        AudienceId = aud.Id,
                                        CreatedUtc = activity.CreatedUtc,
                                    });
                                }

                                uow.Commit();

                                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                                return BadRequest(ModelState);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(MessageType.LoginProviderNotFound.ToString(), $"No login provider for user:{user.Id}");
                            return NotFound(ModelState);
                        }
                    }
                    break;

                case InstanceContext.SystemTest:
                case InstanceContext.IntegrationTest:
                    {
                        /* check if login provider is local or test */
                        if (loginProviders.Where(x => x.Name.Equals(conf["IdentityProvider:BuiltInLoginProviderName"], StringComparison.OrdinalIgnoreCase)).Any()
                            || loginProviders.Where(x => x.Name.StartsWith(conf["IdentityProvider:TestLoginProviderNamePrefix"], StringComparison.OrdinalIgnoreCase)).Any())
                        {
                            /* validate password */
                            if (!PBKDF2.Validate(user.PasswordHashPBKDF2, input.password))
                            {
                                var activity = uow.AuthActivity.Post(
                                    map.Map<tbl_AuthActivity>(new AuthActivityV1()
                                    {
                                        UserId = user.Id,
                                        LoginType = GrantFlowType.ResourceOwnerPasswordV2.ToString(),
                                        LoginOutcome = GrantFlowResultType.Failure.ToString(),
                                        LocalEndpoint = Request.HttpContext.Connection.LocalIpAddress?.ToString() + ":" + Request.HttpContext.Connection.LocalPort,
                                        RemoteEndpoint = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                                    }));

                                foreach (var aud in audiences)
                                {
                                    uow.AuthActivityAudiences.Post(new tbl_AuthActivityAudience
                                    {
                                        AuthActivityId = activity.Id,
                                        AudienceId = aud.Id,
                                        CreatedUtc = activity.CreatedUtc,
                                    });
                                }

                                uow.Commit();

                                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                                return BadRequest(ModelState);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(MessageType.LoginProviderNotFound.ToString(), $"No login provider for user:{user.Id}");
                            return NotFound(ModelState);
                        }

                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
            var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], audiences.Select(x => x.Name).ToList(), rop_claims);

            var acActivity = uow.AuthActivity.Post(
                map.Map<tbl_AuthActivity>(new AuthActivityV1()
                {
                    UserId = user.Id,
                    LoginType = GrantFlowType.ResourceOwnerPasswordV2.ToString(),
                    LoginOutcome = GrantFlowResultType.Success.ToString(),
                    LocalEndpoint = Request.HttpContext.Connection.LocalIpAddress?.ToString() + ":" + Request.HttpContext.Connection.LocalPort,
                    RemoteEndpoint = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                }));

            foreach (var aud in audiences)
            {
                uow.AuthActivityAudiences.Post(new tbl_AuthActivityAudience
                {
                    AuthActivityId = acActivity.Id,
                    AudienceId = aud.Id,
                    CreatedUtc = acActivity.CreatedUtc,
                });
            }

            var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
            var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], audiences.Select(x => x.Name).ToList(), rt_claims);

            uow.Refreshes.Post(
                map.Map<tbl_Refresh>(new RefreshV1()
                {
                    IssuerId = issuer.Id,
                    UserId = user.Id,
                    RefreshType = ConsumerType.User.ToString(),
                    RefreshValue = rt.RawData,
                    IssuedUtc = rt.ValidFrom,
                    ValidFromUtc = rt.ValidFrom,
                    ValidToUtc = rt.ValidTo,
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                }));

            var rtActivity = uow.AuthActivity.Post(
                map.Map<tbl_AuthActivity>(new AuthActivityV1()
                {
                    UserId = user.Id,
                    LoginType = GrantFlowType.RefreshTokenV2.ToString(),
                    LoginOutcome = GrantFlowResultType.Success.ToString(),
                    LocalEndpoint = Request.HttpContext.Connection.LocalIpAddress?.ToString() + ":" + Request.HttpContext.Connection.LocalPort,
                    RemoteEndpoint = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                }));

            foreach (var aud in audiences)
            {
                uow.AuthActivityAudiences.Post(new tbl_AuthActivityAudience
                {
                    AuthActivityId = rtActivity.Id,
                    AudienceId = aud.Id,
                    CreatedUtc = rtActivity.CreatedUtc,
                });
            }

            uow.Commit();

            SetRefreshTokenCookie(rt.RawData, rt.ValidTo);

            var result = new UserJwtV2()
            {
                token_type = "bearer",
                access_token = rop.RawData,
                user = user.UserName,
                client = audiences.Select(x => x.Name).ToList(),
                issuer = issuer.Name + ":" + conf["IdentityTenant:Salt"],
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

            var refreshToken = Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), "Token:Missing");
                return BadRequest(ModelState);
            }

            var refresh = uow.Refreshes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Refresh>()
                .Where(x => x.RefreshValue == refreshToken).ToLambda()).SingleOrDefault();

            if (refresh == null)
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{refreshToken}");
                return NotFound(ModelState);
            }
            else if (!string.Equals(refresh.RefreshType, ConsumerType.User.ToString(), StringComparison.OrdinalIgnoreCase)
                || (refresh.ValidFromUtc >= DateTime.UtcNow || refresh.ValidToUtc <= DateTime.UtcNow))
            {
                ModelState.AddModelError(MessageType.TokenInvalid.ToString(), $"Token:{refreshToken}");
                return BadRequest(ModelState);
            }

            Guid issuerID;
            tbl_Issuer issuer;

            /* resolve identifier to guid */
            if (Guid.TryParse(input.issuer, out issuerID))
                issuer = uow.Issuers.Get(x => x.Id == issuerID).SingleOrDefault();
            else
                issuer = uow.Issuers.Get(x => x.Name == input.issuer).SingleOrDefault();

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

            var user = uow.Users.Get(x => x.Id == refresh.UserId).SingleOrDefault();

            /* check that user exists */
            if (user == null)
            {
                ModelState.AddModelError(MessageType.UserNotFound.ToString(), $"User:{refresh.UserId}");
                return NotFound(ModelState);
            }
            /* check that user is not locked */
            else if (uow.Users.IsLockedOut(user)
                || !user.EmailConfirmed
                || !user.PasswordConfirmed)
            {
                ModelState.AddModelError(MessageType.UserInvalid.ToString(), $"User:{user.Id}");
                return BadRequest(ModelState);
            }

            var clientList = uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                    .Where(x => x.tbl_Roles.Any(y => y.tbl_UserRoles.Any(z => z.UserId == user.Id))).ToLambda());

            var audiences = new List<tbl_Audience>();

            /* check if client is single, multiple or undefined */
            if (string.IsNullOrEmpty(input.client))
                audiences = uow.Audiences.Get(x => clientList.Contains(x)
                    && x.IsLockedOut == false).ToList();
            else
            {
                foreach (string entry in input.client.Split(","))
                {
                    Guid audienceID;
                    tbl_Audience audience;

                    /* resolve identifier to guid */
                    if (Guid.TryParse(entry.Trim(), out audienceID))
                        audience = uow.Audiences.Get(x => x.Id == audienceID).SingleOrDefault();
                    else
                        audience = uow.Audiences.Get(x => x.Name == entry.Trim()).SingleOrDefault();

                    if (audience == null)
                    {
                        ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:{entry}");
                        return NotFound(ModelState);
                    }
                    else if (audience.IsLockedOut
                        || !clientList.Contains(audience))
                    {
                        ModelState.AddModelError(MessageType.AudienceInvalid.ToString(), $"Audience:{audience.Id}");
                        return BadRequest(ModelState);
                    }

                    audiences.Add(audience);
                }
            }

            if (audiences.Count == 0)
            {
                ModelState.AddModelError(MessageType.AudienceNotFound.ToString(), $"Audience:None");
                return BadRequest(ModelState);
            }

            var rop_claims = uow.Users.GenerateAccessClaims(issuer, user);
            var rop = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], audiences.Select(x => x.Name).ToList(), rop_claims);

            var rt_claims = uow.Users.GenerateRefreshClaims(issuer, user);
            var rt = auth.ResourceOwnerPassword(issuer.Name, issuer.IssuerKey, conf["IdentityTenant:Salt"], audiences.Select(x => x.Name).ToList(), rt_claims);

            uow.Refreshes.Post(
                map.Map<tbl_Refresh>(new RefreshV1()
                {
                    IssuerId = issuer.Id,
                    UserId = user.Id,
                    RefreshType = ConsumerType.User.ToString(),
                    RefreshValue = rt.RawData,
                    IssuedUtc = rt.ValidFrom,
                    ValidFromUtc = rt.ValidFrom,
                    ValidToUtc = rt.ValidTo,
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                }));

            var activity = uow.AuthActivity.Post(
                map.Map<tbl_AuthActivity>(new AuthActivityV1()
                {
                    UserId = user.Id,
                    LoginType = GrantFlowType.RefreshTokenV2.ToString(),
                    LoginOutcome = GrantFlowResultType.Success.ToString(),
                    LocalEndpoint = Request.HttpContext.Connection.LocalIpAddress?.ToString() + ":" + Request.HttpContext.Connection.LocalPort,
                    RemoteEndpoint = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                }));

            foreach (var aud in audiences)
            {
                uow.AuthActivityAudiences.Post(new tbl_AuthActivityAudience
                {
                    AuthActivityId = activity.Id,
                    AudienceId = aud.Id,
                    CreatedUtc = activity.CreatedUtc,
                });
            }

            uow.Commit();

            SetRefreshTokenCookie(rt.RawData, rt.ValidTo);

            var result = new UserJwtV2()
            {
                token_type = "bearer",
                access_token = rop.RawData,
                user = user.UserName,
                client = audiences.Select(x => x.Name).ToList(),
                issuer = issuer.Name + ":" + conf["IdentityTenant:Salt"],
                expires_in = (int)(new DateTimeOffset(rop.ValidTo).Subtract(DateTime.UtcNow)).TotalSeconds,
            };

            return Ok(result);
        }

        private void SetRefreshTokenCookie(string refreshToken, DateTime validTo)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = $"{Request.PathBase}/oauth2",
                Expires = validTo
            };
            Response.Cookies.Append("refresh_token", refreshToken, cookieOptions);
        }
    }
}
