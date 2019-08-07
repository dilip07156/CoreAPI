using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class GoAheadSetRes
    {
        public GoAheadSetRes()
        {
            ResponseStatus = new ResponseStatus();
        }
        public mGoAhead mGoAhead { get; set; }
        public ResponseStatus ResponseStatus { get; set; } 
    }

    public class GoAheadNewDeptSetRes
    {
        public GoAheadNewDeptSetRes()
        { 
            ResponseStatus = new ResponseStatus();
        }
        public string GoAheadId { get; set; }
        public string QRFID { get; set; } 
        public ResponseStatus ResponseStatus { get; set; }
    }
}