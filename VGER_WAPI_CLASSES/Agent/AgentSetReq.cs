namespace VGER_WAPI_CLASSES
{
    public class AgentSetReq
    {
        public mCompanies companies { get; set; } = new mCompanies();
        public bool IsCompany { get; set; } = false;
        public bool IsNewCompany { get; set; } = false;
        public bool IsNewBranch { get; set; } = false;
        public bool IsRemoveBranch { get; set; } = false;
        public bool IsNewContact { get; set; } = false;
        public bool IsNewEmergencyContact { get; set; } = false;
        public bool IsNewPaymentTerms { get; set; } = false;
        public bool IsNewTermAndCondition { get; set; } = false;
        public bool IsRemoveCondition { get; set; } = false;
        public bool IsNewPaymentDetail { get; set; } = false;
        public bool IsRemovePaymentDetail { get; set; } = false;
        public bool IsSystemUser { get; set; } = false;
        public string EditUser { get; set; }
        public string LoggedInUserContactId { get; set; }
        public bool IsNewTaxRegistrationDetails { get; set; }
        public bool IsNewCompanyTarget { get; set; }
        public bool IsCompanyTarget { get; set; }
        public bool IsNewContactTarget { get; set; }
        public bool IsContactTarget { get; set; } 
    } 
}
