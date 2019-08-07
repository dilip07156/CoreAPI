using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class DefPersonTypeRes
    {
        public List<mDefPersonType> DefPersonType { get; set; }
        public ResponseStatus ResponseStatus { get; set; }

        public DefPersonTypeRes()
        {
            DefPersonType = new List<mDefPersonType>();
            ResponseStatus = new ResponseStatus();
        }

    }

    public class DefMealTypeRes
    {
        public List<mMealType> DefMealType { get; set; } = new List<mMealType>();
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }
}
