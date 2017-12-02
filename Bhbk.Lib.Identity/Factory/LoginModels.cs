using System;

namespace Bhbk.Lib.Identity.Factory
{
    public class LoginCreate
    {
        public string LoginProvider { get; set; }
    }

    public class LoginModel
    {
        public Guid Id { get; set; }
        public string LoginProvider { get; set; }
    }

    public class LoginUpdate
    {
        public Guid Id { get; set; }
        public string LoginProvider { get; set; }
    }
}
