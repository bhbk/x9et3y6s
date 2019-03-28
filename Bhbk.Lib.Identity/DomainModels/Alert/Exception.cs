using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.DomainModels.Alert
{
    public class ExceptionCreate
    {
        public string Cause { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string Venue { get; set; }
        public List<ClientStackDump> StackTrace { get; set; }

        public class ClientStackDump
        {
            public int ColumnNumber { get; set; }
            public string FileName { get; set; }
            public string FunctionName { get; set; }
            public int LineNumber { get; set; }
            public string Source { get; set; }
        }
    }
}
