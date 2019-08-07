using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ActivitiesSetReq
    {
        public ActivitiesSetReq()
        {
            ActivitiesProperties = new List<ActivitiesProperties>();
        }
        public long QRFID { get; set; }
        public string SaveType { get; set; }
        public List<ActivitiesProperties> ActivitiesProperties { get; set; }
    }
}
