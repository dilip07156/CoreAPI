using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class CostsheetVerSetRes
    {
        public CostsheetVerSetRes()
        {
            ResponseStatus = new ResponseStatus();
        }
        
        public ResponseStatus ResponseStatus { get; set; }
        public string QRFPrice_Id { get; set; }
    }
}
