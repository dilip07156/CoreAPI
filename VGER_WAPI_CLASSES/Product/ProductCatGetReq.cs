using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProductCatGetReq
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public List<string> ProdCatIdList { get; set; }
        public List<string> ProductIdList { get; set; }        
    }
}
