using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class MasterTypeResponse
    {
        public List<Properties> PropertyList;
        public string Status { get; set; }

        public MasterTypeResponse()
        {
            PropertyList = new List<Properties>();
        }
    }
}
