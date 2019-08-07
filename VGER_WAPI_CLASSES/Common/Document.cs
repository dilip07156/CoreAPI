 using System.Collections.Generic;

namespace VGER_WAPI_CLASSES
{
    public class DocumentDetails
    {
        public string Html { get; set; }
        public string DocumentPath { get; set; }
        public string DocumentReference { get; set; }
        public string FullDocumentPath { get; set; }
        public string DocumentName { get; set; }
    }

    public class ZipDetails
    {
        public List<DocumentDetails> DocumentDetails { get; set; }
        public string ZipFileName { get; set; }
        public string ZipFilePath { get; set; }
    } 
}
