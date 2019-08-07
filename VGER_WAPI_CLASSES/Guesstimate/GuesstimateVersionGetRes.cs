using System;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class GuesstimateVersionGetRes
    {
        public List<GuesstimateVersion> GuesstimateVersions { get; set; }
        public ResponseStatus ResponseStatus { get; set; }

        public GuesstimateVersionGetRes()
        {
            GuesstimateVersions = new List<GuesstimateVersion>();
            ResponseStatus = new ResponseStatus();
        }
    }
}
