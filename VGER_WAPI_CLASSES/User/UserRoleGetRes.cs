using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class UserRoleGetRes
    {
        public UserRoleGetRes()
        {
            UserRolesDetails = new List<UserRolesDetails>();
            ResponseStatus = new ResponseStatus();
        }

        public string UserId { get; set; }
        public List<UserRolesDetails> UserRolesDetails { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class UserRolesDetails
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string UserId { get; set; }
        public bool IsRoled { get; set; }
    }
}
