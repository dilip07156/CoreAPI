using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
  public class ProdMarkupsGetRes
    {
        public MarkupDetails MarkupDetails { get; set; } = new MarkupDetails();
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    } 
}
