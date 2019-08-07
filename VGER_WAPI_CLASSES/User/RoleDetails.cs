using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES.User
{
    public class RoleGetRes
    {
        public RoleGetRes()
        {
            RoleDetails = new List<User.RoleDetails>();
            ResponseStatus = new ResponseStatus();
        }
        public List<RoleDetails> RoleDetails { get; set; }
        public ResponseStatus ResponseStatus { get; set; } 

    }
   public  class RoleDetails
    {
        public string RoleId { get; set; }
        public string VoyagerId { get; set; }
        public string RoleName { get; set; }

    }
}
