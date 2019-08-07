using System.Collections.Generic;

namespace VGER_WAPI_CLASSES
{
    public class ProdNationalityGetRes
    {
        public ProdNationalityGetRes()
        {
            NationalityList = new List<AttributeValues>();
            ResponseStatus = new ResponseStatus();
        }
        public List<AttributeValues> NationalityList { get; set; }
        public string CompanyNationality { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class DivisionGetRes
    {
        public DivisionGetRes()
        {
            DivisionList = new List<AttributeValues>();
            ResponseStatus = new ResponseStatus();
        }
        public string CompanyDivision { get; set; }
        public List<AttributeValues> DivisionList { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}
