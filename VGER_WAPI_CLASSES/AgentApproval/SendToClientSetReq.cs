using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// SendToClientGetReq class used for Set Parameters of SendToClient
    /// </summary>
    public class SendToClientSetReq
    {
        /// <summary>
        /// constructor to initialze the fields
        /// </summary>
        public SendToClientSetReq()
        {
            ResponseStatus = new ResponseStatus();
        }

        /// <summary>
        /// QRF ID 
        /// </summary>
        public string QRFID { get; set; }

        /// <summary>
        /// QRF Price ID
        /// </summary>
        public string QRFPriceID { get; set; }

        /// <summary>
        /// ResponseStatus used to main status and errormessage
        /// </summary>
        public ResponseStatus ResponseStatus { get; set; } 

        /// <summary>
        /// contains MailHtml of SendToClient
        /// </summary>
        public string SendToClientHtml { get; set; }

        /// <summary>
        /// UserName 
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Path OF PDF File
        /// </summary>
        public string PDFPath { get; set; }

        /// <summary>
        /// UserName 
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        /// ToCC 
        /// </summary>
        public string ToCC { get; set; }

        public string VoyagerUserId { get; set; }
    }
}