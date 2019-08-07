using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class PositionGetReq
    {
        public string QRFID { get; set; }
        public string PositionId { get; set; }
        public List<ProductType> ProductType { get; set; } = new List<ProductType>();
        public string Type { get; set; } = "";
        public bool IsClone { get; set; } = false;
    }
}
