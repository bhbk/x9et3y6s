using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.Models
{
    public partial class tbl_Exceptions
    {
        public decimal ErrorID { get; set; }
        public DateTime ErrorDate { get; set; }
        public int ErrorNumber { get; set; }
        public string ErrorSeverity { get; set; }
        public int ErrorState { get; set; }
        public string ErrorProcedure { get; set; }
        public int ErrorLine { get; set; }
        public string ErrorMessage { get; set; }
    }
}
