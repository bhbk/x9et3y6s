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

        public UoWService(string connection)
            : this(connection, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UoWService(string connection, IContextService instance)
        {
            switch (instance.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                    {
#if RELEASE

#elif !RELEASE
                        _context.Database.Log = x => Debug.WriteLine(x);
#endif
                        _context = new IdentityEntitiesFactory(connection).Create();
                    }
                    break;

                case InstanceContext.UnitTest:
                    {
                        _context = new IdentityEntitiesFactory(connection).Create();
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            _context.Configuration.LazyLoadingEnabled = false;
            _context.Configuration.ProxyCreationEnabled = true;

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
