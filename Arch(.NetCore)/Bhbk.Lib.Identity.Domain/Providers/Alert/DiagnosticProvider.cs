﻿using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;

namespace Bhbk.Lib.Identity.Domain.Providers.Alert
{
    public class DiagnosticProvider : BaseProvider
    {
        public DiagnosticProvider(IConfiguration conf, IContextService instance)
            : base(conf, instance) { }
    }
}