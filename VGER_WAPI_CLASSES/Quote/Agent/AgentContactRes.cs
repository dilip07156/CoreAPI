using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class AgentContactRes
    {
        public AgentContactRes()
        {
            ContactProperties = new List<ContactProperties>();
            ResponseStatus = new ResponseStatus();
        }

        public string TourNameFlag { get; set; }
        public List<ContactProperties> ContactProperties;
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class ContactProperties
    { 
        public string FullName { get; set; } 
        public string VoyagerContact_Id { get; set; }
    }
}
