using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class BookingDocumentSetRes
    {
        public BookingDocumentSetRes()
        {
            ResponseStatus = new ResponseStatus();
        }
        public ResponseStatus ResponseStatus { get; set; }   
        public long Id { get; set; }
    }
}
