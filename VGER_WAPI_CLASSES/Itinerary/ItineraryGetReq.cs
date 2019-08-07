using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ItineraryGetReq
    {
        public string QRFID { get; set; }

        public string PositionId { get; set; } = null;
        public List<ProductType> ProductType { get; set; } = new List<ProductType>();
        public string Type { get; set; } = "";
        public string Page { get; set; }
        public string editUser { get; set; }
    }
}
