using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
  
    public class CityLookupRequest
    {
        public string QRFID { get; set; }
        public string CountryName { get; set; }
        public string CityName { get; set; }
    }

    /// <summary>
    /// Request format for the City Lookup.
    /// Either of Country Id OR Country Name is required
    /// </summary>
    public class CityLookupRequestMaster
    {
        /// <summary>
        /// Id of the Requested Country in Voyager
        /// </summary>
        public string VoyagerCountry_Id { get; set; }
        /// <summary>
        /// Name of the Country to get the cities
        /// </summary>
        public string CountryName { get; set; }
        /// <summary>
        /// Name of the City (You can pass this only if you need to get detail of specific city)
        /// </summary>
        public string CityName { get; set; }
    }
}
