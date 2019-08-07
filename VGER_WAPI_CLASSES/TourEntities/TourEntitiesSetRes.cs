using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class TourEntitiesSetRes
    {
        public string QRFID { get; set; }
        public List<TourEntities> TourEntities { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}
