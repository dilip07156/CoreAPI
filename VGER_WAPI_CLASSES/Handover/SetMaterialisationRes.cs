using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class SetMaterialisationRes
    {
        public SetMaterialisationRes()
        { 
            mGoAhead = new mGoAhead();
            ResponseStatus = new ResponseStatus();
        }
        public mGoAhead mGoAhead { get; set; } 
        public ResponseStatus ResponseStatus { get; set; }
    }
}
