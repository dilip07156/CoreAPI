using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class FollowUpMasterGetRes
    {
        public List<AttributeValues> FollowUpTaskList { get; set; } = new List<AttributeValues>();

        public List<CompanyContacts> InternalUserList { get; set; } = new List<CompanyContacts>();

        public List<CompanyContacts> ExternalUserList { get; set; } = new List<CompanyContacts>();

        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }
}
