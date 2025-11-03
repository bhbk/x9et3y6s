using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Domain.Builders;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Lib.Identity.Domain.Infrastructure
{
    public class SeedContext : ISeedContext
    {
        private readonly IUnitOfWork _uow;
        private readonly List<Action> _seedActions = new List<Action>();
        private readonly List<tbl_Setting> _createdSettings = new List<tbl_Setting>();

        private tbl_Issuer _issuer;
        private readonly Dictionary<string, tbl_Audience> _audiences = new Dictionary<string, tbl_Audience>();
        private readonly Dictionary<string, tbl_Role> _roles = new Dictionary<string, tbl_Role>();
        private readonly Dictionary<string, tbl_User> _users = new Dictionary<string, tbl_User>();
        private readonly Dictionary<string, tbl_LoginProvider> _loginProviders = new Dictionary<string, tbl_LoginProvider>();
        private readonly Dictionary<string, tbl_Claim> _claims = new Dictionary<string, tbl_Claim>();
        private readonly Dictionary<string, tbl_Url> _urls = new Dictionary<string, tbl_Url>();
        private readonly List<tbl_State> _states = new List<tbl_State>();
        private readonly List<tbl_Refresh> _refreshes = new List<tbl_Refresh>();
        private readonly Dictionary<string, string> _userPasswords = new Dictionary<string, string>();

        private bool _seeded = false;
        private bool _disposed = false;

        public tbl_Issuer Issuer => _issuer;
        public IReadOnlyDictionary<string, tbl_Audience> Audiences => _audiences;
        public IReadOnlyDictionary<string, tbl_Role> Roles => _roles;
        public IReadOnlyDictionary<string, tbl_User> Users => _users;
        public IReadOnlyDictionary<string, tbl_LoginProvider> LoginProviders => _loginProviders;
        public IReadOnlyDictionary<string, tbl_Claim> Claims => _claims;
        public IReadOnlyDictionary<string, tbl_Url> Urls => _urls;
        public IReadOnlyList<tbl_State> States => _states.AsReadOnly();
        public IReadOnlyList<tbl_Refresh> Refreshes => _refreshes.AsReadOnly();
        public IReadOnlyDictionary<string, string> UserPasswords => _userPasswords;

        public SeedContext(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public SeedContext WithIssuer(Action<IssuerBuilder> configure)
        {
            _seedActions.Add(() =>
            {
                var builder = new IssuerBuilder();
                configure(builder);

                var issuerName = builder.GetName();

                _issuer = _uow.Issuers.Get(QueryExpressionFactory.GetQueryExpression<tbl_Issuer>()
                    .Where(x => x.Name == issuerName).ToLambda())
                    .FirstOrDefault();

                if (_issuer == null)
                {
                    _issuer = _uow.Issuers.Post(builder.Build());
                    _uow.Commit();
                }

                var settings = builder.BuildSettings(_issuer.Id);
                foreach (var setting in settings)
                {
                    var existing = _uow.Settings.Get(x => x.IssuerId == _issuer.Id && x.ConfigKey == setting.ConfigKey)
                        .FirstOrDefault();

                    if (existing == null)
                    {
                        _uow.Settings.Post(setting);
                        _createdSettings.Add(setting);
                    }
                }

                if (settings.Any())
                    _uow.Commit();
            });

            return this;
        }

        public SeedContext WithAudience(Action<AudienceBuilder> configure)
        {
            _seedActions.Add(() =>
            {
                if (_issuer == null)
                    throw new InvalidOperationException("Issuer must be created before Audience");

                var builder = new AudienceBuilder();
                builder.ForIssuer(_issuer.Id);
                configure(builder);

                var audienceName = builder.GetName();

                var audience = _uow.Audiences.Get(QueryExpressionFactory.GetQueryExpression<tbl_Audience>()
                    .Where(x => x.Name == audienceName).ToLambda())
                    .FirstOrDefault();

                if (audience == null)
                {
                    audience = _uow.Audiences.Post(builder.Build(), builder.GetPassword());
                    _uow.Commit();
                }

                _audiences[audienceName] = audience;

                /* create roles for this audience */
                foreach (var roleName in builder.GetRoleNames())
                {
                    var role = _uow.Roles.Get(x => x.Name == roleName).FirstOrDefault();

                    if (role == null)
                    {
                        role = _uow.Roles.Post(new RoleBuilder()
                            .ForAudience(audience.Id)
                            .WithName(roleName)
                            .IsDeletable(true)
                            .Build());
                        _uow.Commit();
                    }

                    _roles[roleName] = role;
                }
            });

            return this;
        }

        public SeedContext WithRole(Action<RoleBuilder> configure)
        {
            _seedActions.Add(() =>
            {
                if (!_audiences.Any())
                    throw new InvalidOperationException("Audience must be created before Role");

                var builder = new RoleBuilder();
                builder.ForAudience(_audiences.Values.First().Id);
                configure(builder);

                var roleName = builder.GetName();

                var role = _uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<tbl_Role>()
                    .Where(x => x.Name == roleName).ToLambda())
                    .FirstOrDefault();

                if (role == null)
                {
                    role = _uow.Roles.Post(builder.Build());
                    _uow.Commit();
                }

                _roles[roleName] = role;
            });

            return this;
        }

        public SeedContext WithLoginProvider(Action<LoginProviderBuilder> configure)
        {
            _seedActions.Add(() =>
            {
                var builder = new LoginProviderBuilder();
                configure(builder);

                var providerName = builder.GetName();

                var provider = _uow.LoginProviders.Get(QueryExpressionFactory.GetQueryExpression<tbl_LoginProvider>()
                    .Where(x => x.Name == providerName).ToLambda())
                    .FirstOrDefault();

                if (provider == null)
                {
                    provider = _uow.LoginProviders.Post(builder.Build());
                    _uow.Commit();
                }

                _loginProviders[providerName] = provider;
            });

            return this;
        }

        public SeedContext WithClaim(Action<ClaimBuilder> configure)
        {
            _seedActions.Add(() =>
            {
                if (_issuer == null)
                    throw new InvalidOperationException("Issuer must be created before Claim");

                var builder = new ClaimBuilder();
                builder.ForIssuer(_issuer.Id);
                configure(builder);

                var claimType = builder.GetClaimType();

                var claim = _uow.Claims.Get(QueryExpressionFactory.GetQueryExpression<tbl_Claim>()
                    .Where(x => x.Type == claimType).ToLambda())
                    .FirstOrDefault();

                if (claim == null)
                {
                    claim = _uow.Claims.Post(builder.Build());
                    _uow.Commit();
                }

                _claims[claimType] = claim;
            });

            return this;
        }

        public SeedContext WithUser(Action<UserBuilder> configure)
        {
            _seedActions.Add(() =>
            {
                var builder = new UserBuilder();
                configure(builder);

                var userName = builder.GetUserName();
                var password = builder.GetPassword();

                var user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                    .Where(x => x.UserName == userName).ToLambda())
                    .FirstOrDefault();

                if (user == null)
                {
                    user = _uow.Users.Post(builder.Build(), password);
                    _uow.Commit();

                    _uow.Users.SetConfirmedEmail(user, true);
                    _uow.Users.SetConfirmedPhoneNumber(user, true);
                    _uow.Users.SetConfirmedPassword(user, true);
                    _uow.Commit();
                }
                else
                {
                    /* ensure password matches what test expects */
                    _uow.Users.SetPassword(user, password);
                    _uow.Commit();
                }

                _users[userName] = user;
                _userPasswords[userName] = password;

                /* assign roles */
                foreach (var roleName in builder.GetRoleNames())
                {
                    var role = _roles.ContainsKey(roleName) ? _roles[roleName]
                        : _uow.Roles.Get(x => x.Name == roleName).FirstOrDefault();

                    if (role != null && !_uow.Users.IsInRole(user, role))
                    {
                        _uow.Users.AddRole(new tbl_UserRole
                        {
                            UserId = user.Id,
                            RoleId = role.Id,
                            IsDeletable = true,
                            Created = DateTime.UtcNow,
                        });
                        _uow.Commit();
                    }
                }

                /* assign login providers */
                foreach (var provider in _loginProviders.Values)
                {
                    if (!_uow.Users.IsInLoginProvider(user, provider))
                    {
                        _uow.Users.AddLoginProvider(new tbl_UserLoginProvider
                        {
                            UserId = user.Id,
                            LoginProviderId = provider.Id,
                            IsDeletable = true,
                            Created = DateTime.UtcNow,
                        });
                        _uow.Commit();
                    }
                }
            });

            return this;
        }

        public SeedContext WithUrl(Action<UrlBuilder> configure)
        {
            _seedActions.Add(() =>
            {
                if (!_audiences.Any())
                    throw new InvalidOperationException("Audience must be created before Url");

                var audience = _audiences.Values.First();
                var builder = new UrlBuilder();
                builder.ForAudience(audience.Id);
                configure(builder);

                var fullUrl = builder.GetFullUrl();

                var url = _uow.Urls.Get(QueryExpressionFactory.GetQueryExpression<tbl_Url>()
                    .Where(x => x.AudienceId == audience.Id && x.UrlHost + x.UrlPath == fullUrl).ToLambda())
                    .FirstOrDefault();

                if (url == null)
                {
                    url = _uow.Urls.Post(builder.Build());
                    _uow.Commit();
                }

                _urls[fullUrl] = url;
            });

            return this;
        }

        public SeedContext WithState(Action<StateBuilder> configure)
        {
            _seedActions.Add(() =>
            {
                if (_issuer == null)
                    throw new InvalidOperationException("Issuer must be created before State");

                var builder = new StateBuilder();
                builder.ForIssuer(_issuer.Id);

                if (_audiences.Any())
                    builder.ForAudience(_audiences.Values.First().Id);

                if (_users.Any())
                    builder.ForUser(_users.Values.First().Id);

                configure(builder);

                var state = _uow.States.Post(builder.Build());
                _uow.Commit();

                _states.Add(state);
            });

            return this;
        }

        public SeedContext WithRefresh(Action<RefreshBuilder> configure)
        {
            _seedActions.Add(() =>
            {
                if (_issuer == null)
                    throw new InvalidOperationException("Issuer must be created before Refresh");

                var builder = new RefreshBuilder();
                builder.ForIssuer(_issuer.Id);

                if (_audiences.Any())
                    builder.ForAudience(_audiences.Values.First().Id);

                if (_users.Any())
                    builder.ForUser(_users.Values.First().Id);

                configure(builder);

                var refresh = _uow.Refreshes.Post(builder.Build());
                _uow.Commit();

                _refreshes.Add(refresh);
            });

            return this;
        }

        public ISeedContext Seed()
        {
            if (_seeded)
                return this;

            foreach (var action in _seedActions)
            {
                action();
            }

            _seeded = true;
            return this;
        }

        public void Destroy()
        {
            if (_disposed)
                return;

            try
            {
                foreach (var refresh in _refreshes)
                {
                    var found = _uow.Refreshes.Get(x => x.Id == refresh.Id).FirstOrDefault();
                    if (found != null)
                        _uow.Refreshes.Delete(found);
                }
                if (_refreshes.Any())
                    _uow.Commit();

                foreach (var state in _states)
                {
                    var found = _uow.States.Get(x => x.Id == state.Id).FirstOrDefault();
                    if (found != null)
                        _uow.States.Delete(found);
                }
                if (_states.Any())
                    _uow.Commit();

                foreach (var user in _users.Values.Where(u => u.IsDeletable))
                {
                    var found = _uow.Users.Get(x => x.Id == user.Id).FirstOrDefault();
                    if (found != null)
                        _uow.Users.Delete(found);
                }
                if (_users.Any())
                    _uow.Commit();

                foreach (var claim in _claims.Values.Where(c => c.IsDeletable))
                {
                    var found = _uow.Claims.Get(x => x.Id == claim.Id).FirstOrDefault();
                    if (found != null)
                        _uow.Claims.Delete(found);
                }
                if (_claims.Any())
                    _uow.Commit();

                foreach (var provider in _loginProviders.Values.Where(p => p.IsDeletable))
                {
                    var found = _uow.LoginProviders.Get(x => x.Id == provider.Id).FirstOrDefault();
                    if (found != null)
                        _uow.LoginProviders.Delete(found);
                }
                if (_loginProviders.Any())
                    _uow.Commit();

                foreach (var url in _urls.Values.Where(u => u.IsDeletable))
                {
                    var found = _uow.Urls.Get(x => x.Id == url.Id).FirstOrDefault();
                    if (found != null)
                        _uow.Urls.Delete(found);
                }
                if (_urls.Any())
                    _uow.Commit();

                foreach (var role in _roles.Values.Where(r => r.IsDeletable))
                {
                    var found = _uow.Roles.Get(x => x.Id == role.Id).FirstOrDefault();
                    if (found != null)
                        _uow.Roles.Delete(found);
                }
                if (_roles.Any())
                    _uow.Commit();

                foreach (var audience in _audiences.Values.Where(a => a.IsDeletable))
                {
                    var found = _uow.Audiences.Get(x => x.Id == audience.Id).FirstOrDefault();
                    if (found != null)
                        _uow.Audiences.Delete(found);
                }
                if (_audiences.Any())
                    _uow.Commit();

                foreach (var setting in _createdSettings)
                {
                    var found = _uow.Settings.Get(x => x.Id == setting.Id).FirstOrDefault();
                    if (found != null)
                        _uow.Settings.Delete(found);
                }
                if (_createdSettings.Any())
                    _uow.Commit();

                if (_issuer != null && _issuer.IsDeletable)
                {
                    var found = _uow.Issuers.Get(x => x.Id == _issuer.Id).FirstOrDefault();
                    if (found != null)
                    {
                        _uow.Issuers.Delete(found);
                        _uow.Commit();
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                /* shared in-memory DB - another test may have deleted these */
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    Destroy();

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
