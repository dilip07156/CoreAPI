using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ContactDetailsRequest
    {
        public string Email { get; set; }
        public string TEL { get; set; }
        public string WEB { get; set; }
        public string MOBILE { get; set; }
        public string FAX { get; set; }
        public bool IsUpdateCompany { get; set; }
        public mUsers Users { get; set; }
        
    }
}
