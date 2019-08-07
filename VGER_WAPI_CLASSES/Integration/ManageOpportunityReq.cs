using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ManageOpportunityReq
    {
        public ManageOpportunityReq()
        {
            CredentialInfo = new IntegrationLoginRequest();
            OpportunityInfo = new OpportunityInfo();
        }
        public IntegrationLoginRequest CredentialInfo { get; set; }

        public OpportunityInfo OpportunityInfo { get; set; }

        public string CreatedUser { get; set; }

        public string Application { get; set; }

        public string LoggedInUserContactId { get; set; }

        public string Token { get; set; }
    }
}
