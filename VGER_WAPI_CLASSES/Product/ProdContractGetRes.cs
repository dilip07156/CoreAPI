using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProdContractGetRes
    {
        public List<ProductContractInfo> ProductContractInfo { get; set; }
        public ResponseStatus ResponseStatus { get; set; }

        public ProdContractGetRes()
        {
            ProductContractInfo = new List<ProductContractInfo>();
            ResponseStatus = new ResponseStatus();
        }
    }
}
