using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class GoAheadSetReq
    {
        public mGoAhead mGoAhead { get; set; }
        public string Type { get; set; }
        public long DepatureId { get; set; }
		public string UserEmail { get; set; }
        public string VoyagerUserId { get; set; }
    }

    public class GoAheadNewDeptSetReq
    {
        public GoAheadNewDeptSetReq()
        {
            ExisitingDepatures = new ExisitingDepatures();
            NewDepatures = new List<NewDepatures>();
            ResponseStatus = new ResponseStatus();
        }
        public string GoAheadId { get; set; }
        public string QRFID { get; set; } 
        public string UserEmail { get; set; }
        public ExisitingDepatures ExisitingDepatures { get; set; }
        public List<NewDepatures> NewDepatures { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}
