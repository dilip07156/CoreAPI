using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class BookingDocumentGetRes
    {
        public BookingDocumentGetRes()
        {
            ResponseStatus = new ResponseStatus();
        }

        public string Document_Id { get; set; }
        public string File_Path { get; set; }
        public string FileCreationDate { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}
