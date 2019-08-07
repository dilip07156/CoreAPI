using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProductSupplierGetReq
    {
        public string ProductId { get; set; }
        public List<string> ProductIdList { get; set; }
        public bool IsContractRateRequired { get; set; }
    }
}
