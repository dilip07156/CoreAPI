using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// Request format for the Country Lookup.
    /// You can pass the blank Request Object to get the dump of all countries together.
    /// </summary>
    public class CountryLookupRequest
    {
        /// <summary>
        /// Name of the Country
        /// </summary>
        public string CountryName { get; set; }
        /// <summary>
        /// Flag to Include Cities also in response (Accepted Values are Y / N)
        /// </summary>
        public string IncludeCities { get; set; }
    }
    /// <summary>
    /// Response format for the Country Lookup
    /// </summary>
    public class CountryLookupResponse
    {
        public CountryLookupResponse()
        {
            CountryLookupProperties = new List<CountryLookupProperties>();
            ResponseStatus = new ResponseStatus();
        }
        /// <summary>
        /// List of Countries
        /// </summary>
        public List<CountryLookupProperties> CountryLookupProperties;
        /// <summary>
        /// Status of Look up Attempt
        /// </summary>
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class CountryLookupProperties
    {
        /// <summary>
        /// Full Name along with Continent. "CountryName, ContinentName" Format
        /// </summary>
        public string Lookup { get; set; }
        /// <summary>
        /// Name of the Country
        /// </summary>
        public string CountryName { get; set; }
        /// <summary>
        /// Name of the Continent
        /// </summary>
        public string ContinentName { get; set; }
        /// <summary>
        /// Unique Id of the Country in Voyager
        /// </summary>
        public string Voyager_Resort_Id { get; set; }
        /// <summary>
        /// Nationality name of the Country
        /// </summary>
        public string Nationality { get; set; }
        /// <summary>
        /// Official Language spoken in the Country
        /// </summary>
        public string Language { get; set; }
        /// <summary>
        /// List of Cities in the given Country
        /// </summary>
        public List<CityLookupProperties> Cities { get; set; }

    }
}
