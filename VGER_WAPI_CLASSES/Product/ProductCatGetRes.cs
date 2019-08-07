using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProductCatGetRes
    {
        public ProductCatGetRes()
        {
            ProdCategoryDetails = new List<ProdCategoryDetails>();
            ResponseStatus = new ResponseStatus();
        }

        public List<ProdCategoryDetails> ProdCategoryDetails { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        public string ProductId { get; set; }
    }

    public class ProdCategoryDetails
    {
        public string ProductId { get; set; }
        public string ProductCategoryName { get; set; }
        public string ProductCategoryId { get; set; } 
    }
}
