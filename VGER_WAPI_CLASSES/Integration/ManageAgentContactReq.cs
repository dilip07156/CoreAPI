using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ManageAgentContactReq
    {
        public ManageAgentContactReq()
        {
            CredentialInfo = new IntegrationLoginRequest();
            ContactMappingInfo = new ManageAgentContactMapping();
        }

        public IntegrationLoginRequest CredentialInfo { get; set; }
        public string CompanyId { get; set; }
        public string SelectedCompanyId { get; set; }
        public string SelectedContactId { get; set; }
        public ManageAgentContactMapping ContactMappingInfo { get; set; }
        public string LoggedInUserContactId { get; set; }
        public string Token { get; set; }
    }

    public class ManageAgentContactMapping
    {
        public string Application_Id { get; set; }
        public string Application { get; set; }
        public string PartnerEntityCode { get; set; }
        public string PartnerEntityName { get; set; }

        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public string Telephone { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
