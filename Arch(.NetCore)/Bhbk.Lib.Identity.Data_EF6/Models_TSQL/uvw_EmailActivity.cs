//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Bhbk.Lib.Identity.Data_EF6.Models_TSQL
{
    using System;
    using System.Collections.Generic;
    
    public partial class uvw_EmailActivity
    {
        public System.Guid Id { get; set; }
        public System.Guid EmailId { get; set; }
        public string SendgridId { get; set; }
        public string SendgridStatus { get; set; }
        public System.DateTimeOffset StatusAtUtc { get; set; }
    }
}