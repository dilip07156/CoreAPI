using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// This class used for to send the response when Accept of SentToClient Mail is done
    /// </summary>
    public class AcceptSendToClientSetRes
    {
        /// <summary>
        /// constructor to initialze the fields
        /// </summary>
        public AcceptSendToClientSetRes()
        {
            ResponseStatus = new ResponseStatus();
            CostingGetRes = new CostingGetRes();
        }

        /// <summary>
        /// ResponseStatus used to main status and errormessage
        /// </summary>
        public ResponseStatus ResponseStatus { get; set; }

        /// <summary>
        /// UserName used to maintain the username of Accepting Mail
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// QRF ID 
        /// </summary>
        public string QRFID { get; set; }

        /// <summary>
        /// QRF Price ID
        /// </summary>
        public string QRFPriceID { get; set; }

        /// <summary>
        /// Status captures the wheather the Mail Exists / Mail Notexists / QRFID Invalid/ QRFPriceID Invalid
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Sales Officer Name
        /// </summary>
        public string SalesOfficerName { get; set; }

        /// <summary>
        /// SalesOfficerPhone contains either Telephone or MobileNo
        /// </summary>
        public string SalesOfficerPhone { get; set; }

        /// <summary>
        /// CostingGetRes details
        /// </summary>
        public CostingGetRes CostingGetRes { get; set; }

        /// <summary>
        /// MailStatus captures the Mail Status
        /// </summary>
        public string MailStatus { get; set; }
    }
}