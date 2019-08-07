using System;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class PositionFOCSetReq
    {
        public bool StandardFOC { get; set; }
        public List<mPositionFOC> PositionFOC { get; set; } = new List<mPositionFOC>();
        public bool IsClone { get; set; } = false;
    }

    public class PositionFOCSetAllReq
    {
        public List<mPosition> mPosition { get; set; } = new List<mPosition>();
        public mQuote mQuote { get; set; } = new mQuote();
    }
}
