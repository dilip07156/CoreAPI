using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// SendToClientGetReq class used for Get Parameters of SendToClient
    /// </summary>
    public class SendToClientGetReq
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
        /// Document_Id
        /// </summary>
        public string Document_Id { get; set; }

        /// <summary>
        /// User Name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// MailStatus identify the wheather its suggest or accept or any other
        /// </summary>
        public string MailStatus { get; set; }

        /// <summary>
        /// Comments
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Type used for checking weather its click on AgentAccept from UI 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// User Email
        /// </summary>
        public string UserEmailId { get; set; }

        /// <summary>
        /// User CompanyId (i.e, login user's company id)
        /// </summary>
        public string UserCompanyId { get; set; }

        /// <summary>
        /// User Id
        /// </summary>
        public string VoyagerUserId { get; set; }
    }
}