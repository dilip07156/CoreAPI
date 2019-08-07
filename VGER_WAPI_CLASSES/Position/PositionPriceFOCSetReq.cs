using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class PositionPriceFOCSetReq
    {
        public string PositionId { get; set; } 
        public List<ProductRangeInfo> ProductRangeInfo { get; set; } = new List<ProductRangeInfo>();
        public string QRFID { get; set; }
        public List<string> ProductTypeList { get; set; }
        public List<string> PositionIdList { get; set; }
        public bool IsPrice { get; set; }
        public bool IsFOC { get; set; }
        public bool IsClone { get; set; } = false;
        public string LoginUserId { get; set; }
    }

    public class ProductRangeInfo
    {
        public string VoyagerProductRange_Id { get; set; }
        public string ProductRangeCode { get; set; }
        public string ProductType_Id { get; set; }
        public string PersonType { get; set; }
        public string ProductMenu { get; set; }
    }
}
