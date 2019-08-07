using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class PositionPriceGetRes
    {
        public bool? StandardPrice { get; set; }
        public List<mPositionPrice> PositionPrice { get; set; }
        public List<PositionPriceData> PositionPriceData { get; set; }
        public bool IsSalesOfficeUser { get; set; }

        public ResponseStatus ResponseStatus { get; set; }

        public PositionPriceGetRes()
        {
            PositionPrice = new List<mPositionPrice>();
            PositionPriceData = new List<PositionPriceData>();
            ResponseStatus = new ResponseStatus();
        }
    }
}
