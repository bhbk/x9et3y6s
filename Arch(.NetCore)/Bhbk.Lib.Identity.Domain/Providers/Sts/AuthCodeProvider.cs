﻿using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Sts
{
    public class AuthCodeProvider : BaseProvider
    {
        public AuthCodeProvider(IConfiguration conf, IContextService env)
            : base(conf, env) { }
    }
}
