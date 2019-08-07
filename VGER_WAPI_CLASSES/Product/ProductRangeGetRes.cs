using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProductRangeGetRes
    {
        public ProductRangeGetRes()
        {
            ProductRangeDetails = new List<ProductRangeDetails>();
            ResponseStatus = new ResponseStatus();
            DefProdRangelist = new List<ProductRangeDetails>();
        }

        public List<ProductRangeDetails> DefProdRangelist { get; set; }
        public List<ProductRangeDetails> ProductRangeDetails { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        public string ProductId { get; set; }
        public string ProductCatId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierId { get; set; }
    }

    public class ProductRangeDetails
    { 
        public string VoyagerProductRange_Id { get; set; }
        public string ProductRangeName { get; set; }
        public string ProductRangeCode { get; set; }
        public string ProductCategoryName { get; set; }
        public string ProductCategoryId { get; set; }
        public string PersonType { get; set; }
        public string AgeRange { get; set; }
        public bool? AdditionalYN { get; set; }
        public string ProductId { get; set; }
        public string ProductMenu { get; set; }
    }
}
