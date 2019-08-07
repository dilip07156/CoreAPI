using System.Collections.Generic;

namespace VGER_WAPI_CLASSES
{
    public class TaxRegestrationDetails_Res
    {
        public TaxRegestrationDetails_Res()
        {
            TaxRegestrationDetails = new List<TaxRegestrationDetails>();
            ResponseStatus = new ResponseStatus();

        }
        public List<TaxRegestrationDetails> TaxRegestrationDetails { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}
