using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// SendToClientSetRes class used for sending the response of SendToClient
    /// </summary>
    public class SendToClientSetRes
    {
        /// <summary>
        /// constructor to initialize the fields
        /// </summary>
        public SendToClientSetRes()
        {
            ResponseStatus = new ResponseStatus();
        }

        /// <summary>
        /// ResponseStatus maintains the status and ErroMessage of Set Response
        /// </summary>
        public ResponseStatus ResponseStatus { get; set; }

        /// <summary>
        /// QRF ID
        /// </summary>
        public string QRFID { get; set; }

        /// <summary>
        /// QRF Price ID
        /// </summary>
        public string QRFPriceID { get; set; }

        /// <summary>
        /// AgentName / Company Name
        /// </summary>
        public string AgentName { get; set; }

        /// <summary>
        /// Agent TourName
        /// </summary>
        public string TourName { get; set; }

        /// <summary>
        /// ContactPerson Name
        /// </summary>
        public string ContactPerson { get; set; }

        /// <summary>
        /// SalesOfficer EmailID
        /// </summary>
        public string SalesOfficer { get; set; }

        /// <summary>
        /// Conatct Person Email Address
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Travel Date
        /// </summary>
        public string TravellingDate { get; set; }

        /// <summary>
        ///User Destination
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        ///Full Name
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        ///From Mail
        /// </summary>
        public string FromMail { get; set; }

        /// <summary>
        ///Central Mail Box
        /// </summary>
        public List<string> CentralMailBoxList { get; set; }

        /// <summary>
        ///To CC
        /// </summary>
        public List<string> ToCC { get; set; }

        /// <summary>
        ///Selected CC
        /// </summary>
        public string selectedCC { get; set; }
    }
}