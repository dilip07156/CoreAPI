using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class DistanceMatrixGetReq
    {
        public string Units { get; set; }
        public GeocoderLocation Origins { get; set; } = new GeocoderLocation();
        public GeocoderLocation Destinations { get; set; } = new GeocoderLocation();
        public string Transit_Mode { get; set; }
    }
}
