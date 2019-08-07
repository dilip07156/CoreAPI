using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class MarginGetRes
    {
        public MarginGetRes()
        { 
            ResponseStatus = new ResponseStatus();
            Margins = new Margins();
            Margins.Package = new Package();
            Margins.Product = new Product();
        }
        public ResponseStatus ResponseStatus { get; set; }
        public string QRFID { get; set; }
        public Margins Margins { get; set; }
    }
}
