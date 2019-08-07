using System;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class PositionSetRes
    {
        public PositionSetRes()
        {
            ResponseStatus = new ResponseStatus();
            mPosition = new List<mPosition>();
            PositionDetails = new List<PositionDetails>();
        }
        public ResponseStatus ResponseStatus { get; set; }
        public string PositionId { get; set; }
        public List<mPosition> mPosition { get; set; }
        public List<PositionDetails> PositionDetails { get; set; }
    }

    public class PositionDetails
    {
        public string Days { get; set; }
        public string RoutingDaysID { get; set; }
        public string PositionID { get; set; }
        public string ProductID { get; set; }
    }
}
