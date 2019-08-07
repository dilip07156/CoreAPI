using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class AgentContactPartnerReq
    {
        public AgentContactPartnerReq()
        {
            CredentialInfo = new PartnerLoginRequest();
            AgentContactInfo = new AgentContactPartnerInfo();
        }

        /// <summary>
        /// CredentialInfo is to pass the credential to perform any operation
        /// </summary>
        public PartnerLoginRequest CredentialInfo { get; set; }

        /// <summary>
        /// Agent(Company) Contact Info where partner will need to pass there data to the system.
        /// </summary>
        public AgentContactPartnerInfo AgentContactInfo { get; set; }
    }

    public class AgentContactPartnerInfo
    {
        /// <summary>
        /// ApplicationAgentEntityCode is the part of Unique id of partner to identify the Company in the system (i,e. Voyager).
        /// </summary>
        [Required]
        public string ApplicationAgentEntityCode { get; set; }

        /// <summary>
        /// ApplicationEntityCode is the part of Unique id of partner to identify the Contact of a Company in the system (i,e. Voyager).
        /// </summary>
        [Required]
        public string ApplicationEntityCode { get; set; }
        
        /*
        /// <summary>
        /// Name is Company Name of partner.
        /// </summary>
        [Required]
        public string Name { get; set; }*/

        /// <summary>
        /// Title is Contact Title of partner (i,e. "Mr", "Ms", "Mrs").
        /// </summary>
        [Required]
        [RegularExpression("^(Mr|Mrs|Ms)$", ErrorMessage = "The Title is Invalid.")]
        public string Title { get; set; }

        /// <summary>
        /// FirstName is Contact FirstName of partner.
        /// </summary>
        [Required]
        public string FirstName { get; set; }

        /// <summary>
        /// LastName is Contact LastName of partner.
        /// </summary>
        [Required]
        public string LastName { get; set; }

        /// <summary>
        /// Email is Contact Email of partner.
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Telephone is Contact Telephone of partner.
        /// </summary>
        [Required]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Entered Telephone format is not valid.")]
        public string Telephone { get; set; }
    }
}
