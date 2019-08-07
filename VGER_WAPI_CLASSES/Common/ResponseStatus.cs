using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// Generic Status Format for Requests
    /// </summary>
    public class ResponseStatus
    {
        /// <summary>
        /// Status of Attempt
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// Error Description in case of any failure
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// Error Description in case of any failure, Use for Bridge service
        /// </summary>
        public string StatusMessage { get; set; }        
    }

    /// <summary>
    /// Generic Status Format for Requests
    /// </summary>
    public class ResponseStatusMessage
    {
        /// <summary>
        /// Status of Attempt
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// Error Description in case of any failure
        /// </summary>
        public List<string> ErrorMessage { get; set; }
        /// <summary>
        /// Error Description in case of any failure, Use for Bridge service
        /// </summary>
        public List<string> StatusMessage { get; set; }
    }

    /// <summary>
    /// Generic Status Format for Requests
    /// </summary>
    public class OPSWorkflowResponseStatus
    {
        /// <summary>
        /// Status of Attempt
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// Error Description in case of any failure
        /// </summary>
        public List<string> ErrorMessage { get; set; } = new List<string>();
        /// <summary>
        /// Error Description in case of any failure, Use for Bridge service
        /// </summary>
        public List<string> StatusMessage { get; set; }
 
        public List<DocumentDetails> DocumentDetails { get; set; }
    }
}
