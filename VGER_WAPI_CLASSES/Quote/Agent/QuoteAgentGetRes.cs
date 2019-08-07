using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QuoteAgentGetRes
    {
        public QuoteAgentGetRes()
        {
            QuoteAgentGetProperties = new QuoteAgentGetProperties();
            ResponseStatus = new ResponseStatus();
        }

        public string CompanyDivision { get; set; }
        public QuoteAgentGetProperties QuoteAgentGetProperties { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class QuoteAgentGetProperties
    {
        public QuoteAgentGetProperties()
        {
            AgentInfo = new AgentInfo();
            AgentProductInfo = new AgentProductInfo();
            AgentPassengerInfo = new List<AgentPassengerInfo>();
            AgentRoom = new List<AgentRoom>();
            AgentPaymentInfo = new AgentPaymentInfo();
            DepartureDates = new List<DepartureDates>();
			Division = new List<AttributeValues>();
		}

        public string QRFID { get; set; }

        public string CurrentPipeline { get; set; }
        public string CurrentPipelineStep { get; set; }
        public string CurrentPipelineSubStep { get; set; }
        public string Status { get; set; }
        public string QuoteResult { get; set; }
        public string Remarks { get; set; }
        public string SalesPerson { get; set; }
        public string SalesPersonUserName { get; set; }
        public string SalesPersonCompany { get; set; }
        public string CostingOfficer { get; set; }
        public string ValidForTravel { get; set; }
        public string ValidForAcceptance { get; set; }

        public AgentInfo AgentInfo { get; set; }
        public AgentProductInfo AgentProductInfo { get; set; }
        public List<AgentPassengerInfo> AgentPassengerInfo { get; set; }
        public List<AgentRoom> AgentRoom { get; set; }
        public AgentPaymentInfo AgentPaymentInfo { get; set; }
        public List<DepartureDates> DepartureDates { get; set; }
		public List<AttributeValues> Division { get; set; }

        public bool IsLinkedQRFsExist { get; set; }
    }
}
