using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProdCatDefGetRes
    {
        public ResponseStatus ResponseStatus { get; set; }
        public List<ProdCatDefProperties> ProdCatDefProperties { get; set; }
        public ProdCatDefGetRes()
        {
            ResponseStatus = new ResponseStatus();
            ProdCatDefProperties = new List<ProdCatDefProperties>();
        }
    }

    public class ProdCatDefProperties
    {
        public string VoyagerDefProductCategoryId { get; set; }
        public string Name { get; set; }
    }
}
