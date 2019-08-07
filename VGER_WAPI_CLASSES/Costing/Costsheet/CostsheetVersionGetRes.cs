using System;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class CostsheetVersionGetRes
    {
        public List<CostsheetVersion> CostsheetVersions { get; set; }
        public ResponseStatus ResponseStatus { get; set; }

        public CostsheetVersionGetRes()
        {
            CostsheetVersions = new List<CostsheetVersion>();
            ResponseStatus = new ResponseStatus();
        }
    }
}
