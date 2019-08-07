using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ManageAgentReq
    {
        public ManageAgentReq()
        {
            CredentialInfo = new IntegrationLoginRequest();
        }

        public IntegrationLoginRequest CredentialInfo { get; set; }

        public AgentThirdPartyInfo AgentInfo { get; set; }

        public string CreatedUser { get; set; }

        public string Application { get; set; }

        public string LoggedInUserContactId { get; set; }

        public string Token { get; set; }

    }

    public class AgentThirdPartyInfo
    {
        [Required]
        public string ApplicationEntityCode { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address1 { get; set; }

        public string Address2 { get; set; }

        [Required]
        public string PostCode { get; set; }

        [Required]
        public string City { get; set; }

        public string CityName { get; set; }

        [Required]
        public string Country { get; set; }

        public string CountryName { get; set; }
    }
}
