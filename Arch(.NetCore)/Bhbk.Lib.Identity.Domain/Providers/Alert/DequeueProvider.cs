﻿using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Alert
{
    public class DequeueProvider : BaseProvider
    {
        public DequeueProvider(IConfiguration conf, IContextService env)
            : base(conf, env) { }

    }
}
