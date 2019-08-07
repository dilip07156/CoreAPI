using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class PositionPriceFOCSetRes
    {
        public bool? StandardPrice { get; set; }
        public bool? StandardFOC { get; set; }

        public List<mPositionPrice> PositionPrice { get; set; }
        public List<mPositionFOC> PositionFOC { get; set; }
        public List<mPositionPriceQRF> PositionPriceQRF { get; set; }
        public List<mQRFPositionFOC> PositionFOCQRF { get; set; }
        public ResponseStatus ResponseStatus { get; set; }

        public PositionPriceFOCSetRes()
        {
            PositionPrice = new List<mPositionPrice>(); 
            ResponseStatus = new ResponseStatus();
        }         
    }
}
