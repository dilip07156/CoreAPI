using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProductTypeGetRes
    {
        public ProductTypeGetRes()
        {
            ProductProperties = new List<ProductProperties>();
            ResponseStatus = new ResponseStatus();
        }
        public List<ProductProperties> ProductProperties { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    } 
}
