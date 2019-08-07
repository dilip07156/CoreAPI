using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProdContractGetReq
    {
        public string QRFID { get; set; }
        public List<string> ProductIDList { get; set; }
        public string ProductType { get; set; }
        public string PositionId { get; set; }
        public string SupplierId { get; set; }
        public string AgentId { get; set; }
    }
}
