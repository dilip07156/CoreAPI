using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class SupplierSetRes
    {
        public SupplierSetRes()
        {
            ResponseStatus = new ResponseStatus();           
        }
        public ResponseStatus ResponseStatus { get; set; }
        public string CompanyId { get; set; }
        public string SalesAgentId { get; set; }
    }
}
