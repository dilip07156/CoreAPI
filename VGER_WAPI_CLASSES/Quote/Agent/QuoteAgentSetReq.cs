using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QUOTEAgentSetReq
    {
        public string QRFID { get; set; }

        public string CurrentPipeline { get; set; }
        public string CurrentPipelineStep { get; set; }
        public string CurrentPipelineSubStep { get; set; }
        public string Status { get; set; }
        public string QuoteResult { get; set; }
        public string Remarks { get; set; }
        public string SalesPerson { get; set; }
        public string SalesPersonCompany { get; set; }
        public string SalesPersonUserName { get; set; }
        public string ValidForTravel { get; set; }
        public string ValidForAcceptance { get; set; }
		public string LoggedInUserContact_Id { get; set; }
        public string Module { get; set; }
        public string PartnerEntityCode { get; set; }
        public string Application { get; set; }
        public string Operation { get; set; }
        public string ckLoginUser_Id { get; set; }
        public AgentInfo AgentInfo { get; set; }
        public AgentProductInfo AgentProductInfo { get; set; }
        public List<AgentPassengerInfo> AgentPassengerInfo { get; set; }
        public List<AgentRoomInfo> AgentRoomInfo { get; set; }
        public AgentPaymentInfo AgentPaymentInfo { get; set; }
    }
}
 