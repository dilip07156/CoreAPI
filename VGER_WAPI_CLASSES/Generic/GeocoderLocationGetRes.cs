using System;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class GeocoderLocationGetRes
    {
        public GeocoderLocation GeocoderLocation { get; set; } = new GeocoderLocation();
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }
    
}
