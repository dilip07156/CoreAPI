using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class TourEntitiesSetReq
    { 
        public string QRFID { get; set; }
        public string UserName { get; set; }
        public List<TourEntities> TourEntities { get; set; } 
        public string VoyagerUserID { get; set; }
    }
}
