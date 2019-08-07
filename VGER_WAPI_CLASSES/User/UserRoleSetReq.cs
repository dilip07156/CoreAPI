using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class UserRoleSetReq
    {
        public List<UserRolesDetails> UserRoleDetailsList { get; set; } = new List<UserRolesDetails>();
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EditUser { get; set; }
        public string CompanyId { get; set; }
        public string ContactId { get; set; }
    }
}
