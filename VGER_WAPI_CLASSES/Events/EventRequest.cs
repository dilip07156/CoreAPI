using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// EventRequest defined for taking the City Name and date
    /// </summary>
    public class EventRequest
    {
        /// <summary>
        /// Name of City
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// date of event
        /// </summary>
        public DateTime Date { get; set; }
    }
}
