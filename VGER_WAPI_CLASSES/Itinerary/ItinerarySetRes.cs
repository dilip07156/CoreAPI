using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ItinerarySetRes
    {
        public ItinerarySetRes()
        {
            ResponseStatus = new ResponseStatus();
        }
        public ResponseStatus ResponseStatus { get; set; }
        public string ItineraryId { get; set; }
        //public List<mItinerary> mItinerary { get; set; }
    }
}
