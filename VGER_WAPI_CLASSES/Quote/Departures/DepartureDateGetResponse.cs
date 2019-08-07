using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class DepartureDateGetResponse
    {
        public List<DepartureDates> DepartureDates { get; set; }
        public string Status { get; set; }

        public DepartureDateGetResponse()
        {
            DepartureDates = new List<DepartureDates>();
        }
    }
}
