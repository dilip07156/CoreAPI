using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class DocumentStoreSetRes
    {
       public DocumentStoreSetRes()
        {
            ResponseStatus = new ResponseStatus();
        }

        public string DocumentId { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}