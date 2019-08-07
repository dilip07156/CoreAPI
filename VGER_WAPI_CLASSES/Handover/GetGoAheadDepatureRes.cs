using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class GetGoAheadDepatureRes
    {
        public GetGoAheadDepatureRes()
        {
            mGoAhead = new mGoAhead();
            ResponseStatus = new ResponseStatus();
        }
        public mGoAhead mGoAhead { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        public List<AttributeValues> ChildTypeList = new List<AttributeValues>();
    }
}
