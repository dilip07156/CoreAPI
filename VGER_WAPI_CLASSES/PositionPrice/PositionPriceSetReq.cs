using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class PositionPriceSetReq
    {
        public bool StandardPrice { get; set; }
        public List<mPositionPrice> PositionPrice { get; set; } = new List<mPositionPrice>();
        public bool IsClone { get; set; } = false;
        public string QRFID { get; set; }
        public string VoyagerUserID { get; set; }
    }

    public class PositionPriceSetAllReq
    {
        public List<mPosition> mPosition { get; set; } = new List<mPosition>();
        public mQuote mQuote { get; set; } = new mQuote(); 
    }
}
