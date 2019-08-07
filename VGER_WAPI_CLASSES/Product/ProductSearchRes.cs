using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProductSearchRes
    {
        public ProductSearchRes()
        {
            ProductSearchDetails = new List<ProductSearchDetails>();
            ResponseStatus = new ResponseStatus();
        }
        public List<ProductSearchDetails> ProductSearchDetails { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }

    
     
}
