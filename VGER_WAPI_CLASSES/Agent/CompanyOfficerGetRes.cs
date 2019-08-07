using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class CompanyOfficerGetRes
    {
        public List<CompanyContacts> ContactDetails { get; set; }
		public List<ChildrenCompanies> Branches { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        
        public CompanyOfficerGetRes()
        {
            ContactDetails = new List<CompanyContacts>();
			Branches = new List<ChildrenCompanies>();
			ResponseStatus = new ResponseStatus();
        }
    }
}
