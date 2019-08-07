using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// Get details of current company
    /// </summary>
    public class CompanyDetailsRes
    {
        public string SystemPhone { get; set; }
        public string SystemEmail { get; set; }
        public string SystemWebsite { get; set; }
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }
}
