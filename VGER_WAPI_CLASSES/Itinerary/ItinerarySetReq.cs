using System;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class ItinerarySetReq
    {
        public bool IsNewVersion { get; set; } = false;
        public bool IsExtraItineraryElement { get; set; } = false;
        public mItinerary itinerary { get; set; } = new mItinerary();
        public string QRFId { get; set; }
        public string ItineraryId { get; set; }
        public string ItineraryDaysId { get; set; }
        public string PositionId { get; set; }
        public bool IsDeleted { get; set; }
        public string TLRemarks { get; set; }
        public string OPSRemarks { get; set; }
        public string VoyagerUserId { get; set; }
    }
}
