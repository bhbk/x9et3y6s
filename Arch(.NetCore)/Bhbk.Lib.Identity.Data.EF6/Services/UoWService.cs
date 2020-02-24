using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Data.EF6.Models;
using Bhbk.Lib.Identity.Data.EF6.Repositories;
using System;
using System.Diagnostics;

namespace Bhbk.Lib.Identity.Data.EF6.Services
{
    public class UoWService : IUoWService, IDisposable
    {
        private readonly IdentityEntities _context;
        public InstanceContext InstanceType { get; private set; }
        public ActivityRepository Activities { get; private set; }
        public IdentityEntities Context
        {
            get
            {
                if (InstanceType == InstanceContext.UnitTest)
                    return _context;

                throw new NotSupportedException();
            }
        }

        public UoWService()
            : this(new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UoWService(IContextService instance)
        {
            switch (instance.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                    {
#if RELEASE

#elif !RELEASE
                        _context.Database.Log = x => Debug.WriteLine(x);
#endif
                        _context = new IdentityEntities();
                    }
                    break;

                case InstanceContext.UnitTest:
                    {
                        _context = new IdentityEntities();
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            _context.Configuration.LazyLoadingEnabled = false;

            InstanceType = instance.InstanceType;

            Activities = new ActivityRepository(_context, instance.InstanceType);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public void Commit()
        {
            _context.SaveChanges();
        }
    }
}
