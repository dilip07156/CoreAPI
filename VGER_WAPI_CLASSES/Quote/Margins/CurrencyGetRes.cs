using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class CurrencyGetRes
    {
        public CurrencyGetRes()
        {
            Currency = new List<Currency>();
            ResponseStatus = new ResponseStatus();
        }
        public List<Currency> Currency { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}
