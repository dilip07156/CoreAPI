using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ActivitiesSetRes
    {
        public ActivitiesSetRes()
        {
            ResponseStatus = new ResponseStatus();
            ActivitiesDetails = new List<ActivitiesProperties>(); 
        }
        public ResponseStatus ResponseStatus { get; set; }
        public long QRFID { get; set; }
        public long ActivityId { get; set; }
        public List<ActivitiesProperties> ActivitiesDetails { get; set; }
    }
}
