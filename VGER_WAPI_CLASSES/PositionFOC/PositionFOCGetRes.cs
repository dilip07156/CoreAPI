using System;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class PositionFOCGetRes
    {
        public bool? StandardFOC { get; set; }
        public List<mPositionFOC> PositionFOC { get; set; }
        public ResponseStatus ResponseStatus { get; set; }

        public PositionFOCGetRes()
        {
            PositionFOC = new List<mPositionFOC>();
            ResponseStatus = new ResponseStatus();
        }
    }
}
