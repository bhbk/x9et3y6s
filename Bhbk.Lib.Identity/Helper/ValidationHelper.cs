using Bhbk.Lib.Identity.Infrastructure;
using System;

namespace Bhbk.Lib.Identity.Helper
{
    public class ValidationHelper
    {
        private IUnitOfWork _uow;

        public ValidationHelper(IUnitOfWork uow)
        {
            if (uow == null)
                throw new ArgumentNullException();

            this._uow = uow;
        }
    }
}
