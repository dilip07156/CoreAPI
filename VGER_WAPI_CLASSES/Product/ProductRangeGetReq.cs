using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProductRangeGetReq
    {
        public string QRFID { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; } 
        public string ProductCatId { get; set; }
        public bool? AdditionalYN { get; set; }
        public List<string> ProductIdList { get; set; }
        public List<string> ProductCatIdList { get; set; }
        public List<string> PersonTypeList { get; set; }
        public List<string> ProductRangeIdList { get; set; }
    }
}
