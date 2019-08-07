using System;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// Generic Status Format for Response
    /// </summary>
    public class CommonResponse
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// QRFID
        /// </summary>
        public string QRFID { get; set; }

        /// <summary>
        /// TourName
        /// </summary>
        public string TourName { get; set; }

        /// <summary>
        /// Generic Status Format for Request/Response
        /// </summary>
        public ResponseStatus ResponseStatus { get; set; }

        /// <summary>
        /// construtor to initialize the fields
        /// </summary>
        public CommonResponse()
        {
            ResponseStatus = new ResponseStatus();
        }
    }
}
