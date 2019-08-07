using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProductSRPSearchRes
    {
        public ProductSRPSearchRes()
        {
            ProductSearchDetails = new List<mProducts_Lite>();
            ResponseStatus = new ResponseStatus();
        }
        public List<mProducts_Lite> ProductSearchDetails { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}
