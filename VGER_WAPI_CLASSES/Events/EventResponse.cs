using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// EventResponse defined for sending the details of Event 
    /// </summary>
    public class EventResponse
    {
        /// <summary>
        /// constructor to initialize the fields
        /// </summary>
        public EventResponse() {
            EventResponseProperties = new List<EventResponseProperties>();
            ResponseStatus = new ResponseStatus();
        }

        public List<EventResponseProperties> EventResponseProperties;

        /// <summary>
        /// ResponseStatus maintains the status and ErroMessage of Set Response
        /// </summary>
        public ResponseStatus ResponseStatus { get; set; }
    }

    /// <summary>
    /// EventResponseProperties contains the fields of City collection
    /// </summary>
    public class EventResponseProperties
    {
        /// <summary>
        /// Name of Event
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of Event
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Name of City
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Event Start date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Event End Date
        /// </summary>
        public DateTime EndDate { get; set; }
    }
}
