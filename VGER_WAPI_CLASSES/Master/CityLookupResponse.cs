using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    
    public class CityLookupResponse
    {
        public CityLookupResponse()
        {
            CityLookupProperties = new List<CityLookupProperties>();
            ResponseStatus = new ResponseStatus();
        }

        public List<CityLookupProperties> CityLookupProperties;
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class CityLookupProperties
    {
        public string Lookup { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public string Voyager_Resort_Id { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

    }
}
