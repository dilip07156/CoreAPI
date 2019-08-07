using System.Collections.Generic;

namespace VGER_WAPI_CLASSES
{
    public class AgentGetRes
    {
        public List<AgentList> AgentList { get; set; }
        public int AgentTotalCount { get; set; }
        public int ProductsTotalCount { get; set; }
        public mCompanies AgentDetails { get; set; }
        //public List<mStatus> StatusList { get; set; }
        //public List<mDefStartPage> DefStartPageList { get; set; }
        //public List<Attributes> DefDocumentTypes { get; set; }
        //public List<Attributes> ProductList { get; set; }
        //public List<Attributes> CountryList { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        public string UserId { get; set; }
        public List<UserRolesDetails> UserRolesDetails { get; set; }
        public mUsers User { get; set; }

        public AgentGetRes()
        {
            AgentList = new List<AgentList>();
            AgentDetails = new mCompanies();
            //StatusList = new List<mStatus>();
            UserRolesDetails = new List<UserRolesDetails>();
            User = new mUsers();
            //DefDocumentTypes = new List<Attributes>();
            //ProductList = new List<Attributes>();
            //CountryList = new List<Attributes>();
            ResponseStatus = new ResponseStatus();
        }
    }

    public class Response
    {
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
        public string SystemCompanyId { get; set; }
    }

    public class TargetGetRes
    {
        public string ActionType { get; set; }
        public string ContactId { get; set; }
        public string CompanyId { get; set; }
        public string Currency { get; set; }
        public List<Targets> TargetList { get; set; } = new List<Targets>();
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }
}
