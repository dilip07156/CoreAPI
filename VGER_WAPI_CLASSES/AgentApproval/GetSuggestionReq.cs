using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// GetSuggestionReq class used for fetching details of Suggestion
    /// </summary>
    public class GetSuggestionReq
    {
        /// <summary>
        /// QRF ID
        /// </summary>
        public string QRFID { get; set; }

        /// <summary>
        /// QRFPrice ID
        /// </summary>
        public string QRFPriceID { get; set; }

        /// <summary>
        /// EmailDetails ID
        /// </summary>
        public string Document_Id { get; set; }

        /// <summary>
        /// to fetch the comments of Suggestion by User
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// MailStatus identify the wheather its suggest or any other
        /// </summary>
        public string MailStatus { get; set; }
    }
}