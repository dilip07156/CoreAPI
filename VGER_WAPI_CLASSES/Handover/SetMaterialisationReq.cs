using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class SetMaterialisationReq
    {
        public SetMaterialisationReq()
        {
            mGoAhead = new mGoAhead(); 
        }
        public mGoAhead mGoAhead { get; set; } 
    }
}
