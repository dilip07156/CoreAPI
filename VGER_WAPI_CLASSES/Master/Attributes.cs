using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class Attributes
    {
        public string AttributeName { get; set; }
        public string Attribute_Id { get; set; }
        public List<AttributeValues> Values;

        public Attributes()
        {
            Values = new List<AttributeValues>();
        }
    }
}
