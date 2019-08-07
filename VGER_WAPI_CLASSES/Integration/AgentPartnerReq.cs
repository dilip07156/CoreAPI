using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class AgentPartnerReq
    {
        public AgentPartnerReq()
        {
            CredentialInfo = new PartnerLoginRequest();
            AgentInfo = new AgentPartnerInfo();
        }

        /// <summary>
        /// CredentialInfo is to pass the credential to perform any operation
        /// </summary>
        public PartnerLoginRequest CredentialInfo { get; set; }

        /// <summary>
        /// Agent(Company) Info where partner will need to pass there data to the system.
        /// </summary>
        public AgentPartnerInfo AgentInfo { get; set; }
    }

    public class PartnerLoginRequest
    {
        /// <summary>
        /// Indicates partner is calling or requesting by.
        /// </summary>
        [Required]
        public string Source { get; set; }

        /// <summary>
        /// Key is the part of authentication with the source.
        /// </summary>
        [Required]
        public string Key { get; set; }

        /// <summary>
        /// User is the part of authentication with the Key.
        /// </summary>
        [Required]
        public string User { get; set; }
    }

    public class AgentPartnerInfo
    {
        /// <summary>
        /// ApplicationEntityCode is the part of Unique id of partner to identify the Company in the system (i,e. Voyager).
        /// </summary>
        [Required]
        public string ApplicationEntityCode { get; set; }

        /// <summary>
        /// Name is Company Name of partner.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Address1 is Company Address1 of partner.
        /// </summary>
        [Required]
        public string Address1 { get; set; }

        /// <summary>
        /// Address2 is Company Address1 of partner.
        /// </summary>
        public string Address2 { get; set; }

        /// <summary>
        /// PostCode is Company PostCode(Zip) of partner.
        /// </summary>
        [Required]
        public string PostCode { get; set; }

        /// <summary>
        /// City is Company City code of partner.
        /// </summary>
        [Required]
        public string City { get; set; }

        /// <summary>
        /// Country is Company Country code of partner.
        /// </summary>
        [Required]
        public string Country { get; set; }
    }
}
