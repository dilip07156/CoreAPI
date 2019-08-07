using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// SendToClientGetRes class used for sending the response of SendToClient
    /// </summary>
    public class SendToClientGetRes
    {
        /// <summary>
        /// constructor to initialize the fields
        /// </summary>
        public SendToClientGetRes()
        {
            ResponseStatus = new ResponseStatus();
            CostingGetRes = new CostingGetRes();
        }

        /// <summary>
        /// ResponseStatus maintains the status and ErroMessage of Get Response
        /// </summary>
        public ResponseStatus ResponseStatus { get; set; }

        /// <summary>
        /// CostingGetRes contains the Costing details
        /// </summary>
        public CostingGetRes CostingGetRes { get; set; }

        /// <summary>
        /// MailStatus identify the wheather its suggest or accept or any other
        /// </summary>
        public string MailStatus { get; set; } 
    }
}