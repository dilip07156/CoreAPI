using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// Geocoder Location class
    /// </summary>
    public class GeocoderLocationGetReq
    {
        /// <summary>
        /// Address 
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Optional Parameter, Type of Address like lodging, restaurant
        /// </summary>
        public string Type { get; set; }
    }
}
