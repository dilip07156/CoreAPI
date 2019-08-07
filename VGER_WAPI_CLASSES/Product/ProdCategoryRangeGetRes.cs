using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProdCategoryRangeGetRes
    {
        public ProdCategoryRangeGetRes()
        {
            ProdCategoryRangeDetails = new List<ProductRangeDetails>();
            ResponseStatus = new ResponseStatus();
        }

        public List<ProductRangeDetails> ProdCategoryRangeDetails { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        public string ProductId { get; set; }
    }

    public class ProdCategoryRangeDetails
    {
        public string ProductCategoryName { get; set; }
        public string ProductCategoryId { get; set; }        
        public string VoyagerProductRange_Id { get; set; }
        public string ProductRangeName { get; set; }
        public string ProductRangeCode { get; set; }
        public bool? AdditionalYN { get; set; }
        public string PersonType { get; set; }
        public string AgeRange { get; set; }
    }
}

