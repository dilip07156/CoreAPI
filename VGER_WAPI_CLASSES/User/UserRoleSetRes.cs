using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class UserRoleSetRes
    {
        public UserRoleSetRes()
        { 
            ResponseStatus = new ResponseStatus();
        }

        public string UserId { get; set; } 
        public ResponseStatus ResponseStatus { get; set; }
    }
}
