using Bhbk.Lib.Identity.Infrastructure;
using Microsoft.Owin.Security.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Sts.Provider
{
    public class CustomAuthorizationCode : IAuthenticationTokenProvider
    {
        private IUnitOfWork _uow;
        private readonly ConcurrentDictionary<string, string> _codes =
                new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        public CustomAuthorizationCode(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public void Create(AuthenticationTokenCreateContext context)
        {
            context.SetToken(Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n"));
            _codes[context.Token] = context.SerializeTicket();
        }

        public Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            string value;

            if (_codes.TryRemove(context.Token, out value))
                context.DeserializeTicket(value);
        }

        public Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            throw new NotImplementedException();
        }
    }
}