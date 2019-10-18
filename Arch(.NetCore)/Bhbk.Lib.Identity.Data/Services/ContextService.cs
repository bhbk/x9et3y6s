using Bhbk.Lib.Common.Primitives.Enums;
using System;

namespace Bhbk.Lib.Identity.Data.Services
{
    public class ContextService : IContextService
    {
        public InstanceContext InstanceType { get; private set; }

        public ContextService(InstanceContext instanceType)
        {
            InstanceType = instanceType;
        }
    }
}
