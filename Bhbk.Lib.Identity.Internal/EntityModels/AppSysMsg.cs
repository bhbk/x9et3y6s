using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class AppSysMsg
    {
        public decimal ErrorId { get; set; }
        public DateTime ErrorDate { get; set; }
        public int ErrorNumber { get; set; }
        public string ErrorSeverity { get; set; }
        public int ErrorState { get; set; }
        public string ErrorProcedure { get; set; }
        public int ErrorLine { get; set; }
        public string ErrorMessage { get; set; }
    }
}
