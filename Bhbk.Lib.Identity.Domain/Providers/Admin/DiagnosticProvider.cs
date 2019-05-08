﻿using Bhbk.Lib.Identity.Data.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Admin
{
    public class DiagnosticProvider : BaseProvider
    {
        public DiagnosticProvider(IConfiguration conf, IContextService instance)
            : base(conf, instance) { }
    }
}
