using System.Collections.Generic;

namespace VGER_WAPI_CLASSES
{
    public class ProductPDPSearchRes
    {
        public List<Products> ProductDetails { get; set; }
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }
}
