using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ActivitiesGetRes
    {
        public ResponseStatus ResponseStatus { get; set; }
        public long QRFID { get; set; }
        public List<ActivitiesProperties> ActivitiesDetails { get; set; }
        public List<string> DaysList { get; set; }

        public ActivitiesGetRes()
        {
            ResponseStatus = new ResponseStatus();
            ActivitiesDetails = new List<ActivitiesProperties>();
            DaysList = new List<string>();
        }
    }
}
