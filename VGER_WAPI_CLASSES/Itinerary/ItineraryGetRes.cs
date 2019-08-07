using System;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class ItineraryGetRes
    {
        public ItineraryGetRes()
        {
            Itinerary = new mItinerary();
            ResponseStatus = new ResponseStatus();
        }

        public string QRFID { get; set; }
        public mItinerary Itinerary { get; set; }
        public ResponseStatus ResponseStatus { get; set; }

        /// <summary>
        /// To fetch data from mQRFPosition
        /// </summary>
        public List<mQRFPosition> mQRFPosition { get; set; }
        public List<mProducts> ProductList { get; set; }
        public List<mQRFPosition> AccomodationDetails { get; set; }
      
        public List<AttributeValues> DaysList { get; set; }
        public List<RoutingDays> RoutingDays { get; set; }
        public List<RoutingInfo> RoutingInfo { get; set; }
        public List<ProductType> ProductType { get; set; }
    }
}
