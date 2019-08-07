using System;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class DistanceMatrixGetRes
    {
        public List<string> destination_addresses { get; set; }
        public List<string> origin_addresses { get; set; }
        public List<Row> Rows { get; set; }
        public string status { get; set; }
        public string DestinationCity { get; set; }
        public string OriginCity { get; set; }
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }

    public class Row
    {
        public Element[] Elements { get; set; }

    }

    public class Element
    {
        public TextValue distance { get; set; }
        public TextValue duration { get; set; }
        public string status { get; set; }
    }

    public class TextValue
    {
        public string text { get; set; }
        public string value { get; set; }
    }

}
