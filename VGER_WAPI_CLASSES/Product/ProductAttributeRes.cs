using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProductAttributeRes
    {
        public ResponseStatus ResponseStatus { get; set; }
        public List<ProductAttributeDetails> ProductAttributeDetails { get; set; }
        public ProductAttributeRes()
        {
            ResponseStatus = new ResponseStatus();
            ProductAttributeDetails = new List<ProductAttributeDetails>();
        }
    }

    public class ProductAttributeDetails
    {
        public string AttributeId { get; set; }
        public string Value { get; set; }
    }
}
