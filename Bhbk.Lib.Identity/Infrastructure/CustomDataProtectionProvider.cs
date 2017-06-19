using Microsoft.Owin.Security.DataProtection;
using System.Web.Security;

namespace Bhbk.Lib.Identity.Infrastructure
{
    sealed class CustomDataProtectionProvider : IDataProtectionProvider
    {
        public IDataProtector Create(params string[] uses)
        {
            return new CustomMachineKeyDataProtector(uses);
        }
    }

    sealed class CustomMachineKeyDataProtector : IDataProtector
    {
        private readonly string[] _uses;

        public CustomMachineKeyDataProtector(string[] uses)
        {
            _uses = uses;
        }

        public byte[] Protect(byte[] data)
        {
            return MachineKey.Protect(data, _uses);
        }

        public byte[] Unprotect(byte[] data)
        {
            return MachineKey.Unprotect(data, _uses);
        }
    }
}
