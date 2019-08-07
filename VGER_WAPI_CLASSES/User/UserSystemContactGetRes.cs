using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class UserSystemContactGetRes
    {
        public List<UserSystemContactDetails> UserSystemContactDetails { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}
