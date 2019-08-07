using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class GetCompany_RS
    {
        public decimal ReferenceNumber { get; set; }
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }
}
