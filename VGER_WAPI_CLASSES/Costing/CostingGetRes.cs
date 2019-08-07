using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// CostingGetRes used to send the response of Costing
    /// </summary>
    public class CostingGetRes
    {
        /// <summary>
        /// constructor to initialize the fields
        /// </summary>
        public CostingGetRes()
        {
            CostingGetProperties = new CostingGetProperties();
            ResponseStatus = new ResponseStatus();
        }

        /// <summary>
        /// CostingGetProperties contains Costing details
        /// </summary>
        public CostingGetProperties CostingGetProperties { get; set; }

        /// <summary>
        /// current Pipeline of the QRF
        /// </summary>
        public string EnquiryPipeline { get; set; }
        public bool IsLinkedQRFsExist { get; set; }

        /// <summary>
        /// ResponseStatus maintains the status and ErrorMessage of Set Response
        /// </summary>
        public ResponseStatus ResponseStatus { get; set; }

        /// <summary>
        /// MailStatus identify the wheather its suggest or any other
        /// </summary>
        public string MailStatus { get; set; }

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
    }

    public class CostingGetProperties
    {
        /// <summary>
        /// constructor to initialize the fields
        /// </summary>
        public CostingGetProperties()
        {
            AgentInfo = new AgentInfo();
            AgentProductInfo = new QRFAgentProductInfo();
            AgentPassengerInfo = new List<QRFAgentPassengerInfo>();
            AgentRoom = new List<QRFAgentRoom>();
            DepartureDates = new List<QRFDepartureDates>();
        }

        /// <summary>
        /// QRF ID
        /// </summary>
        public string QRFID { get; set; }

        /// <summary>
        /// QRF Price ID
        /// </summary>
        public string QRFPriceID { get; set; }
        public int VersionId { get; set; }
        public string VersionName { get; set; }
        public string VersionDescription { get; set; }
        public bool IsCurrentVersion { get; set; }

        public string SalesOfficer { get; set; }
        public string SalesOfficerMobile { get; set; }
        public string CostingOfficer { get; set; }
        public string ProductAccountant { get; set; }

        public string ValidForTravel { get; set; }
        public string ValidForAcceptance { get; set; }

        public DateTime? FollowUpCostingOfficer { get; set; }
        public DateTime? FollowUpWithClient { get; set; }

        public AgentInfo AgentInfo { get; set; }
        public QRFAgentProductInfo AgentProductInfo { get; set; }
        public List<QRFAgentPassengerInfo> AgentPassengerInfo { get; set; }
        public List<QRFAgentRoom> AgentRoom { get; set; }
        public List<QRFDepartureDates> DepartureDates { get; set; }

        /// <summary>
        /// Document_Id of mDocumentStore
        /// </summary>
        public string Document_Id { get; set; } 
    }
}
