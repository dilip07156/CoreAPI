using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class AgentContactDetailsRes
    {
        public AgentContactDetailsRes()
        {
            AgentContactDetailsProperties = new AgentContactDetailsProperties();
            ResponseStatus = new ResponseStatus();
        }
        public AgentContactDetailsProperties AgentContactDetailsProperties { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class AgentContactDetailsProperties
    {
        public string MAIL { get; set; }
        public string MOBILE { get; set; }
        public string CommonTitle { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Telephone { get; set; }
        public string Fax { get; set; }
    }
}
