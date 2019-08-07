using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class UserSetRes
    {
        public UserSetRes()
        {
            ResponseStatus = new ResponseStatus();
        }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}
