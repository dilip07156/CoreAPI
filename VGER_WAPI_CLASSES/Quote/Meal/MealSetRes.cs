﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class MealSetRes
    {
        public MealSetRes()
        {
            ResponseStatus = new ResponseStatus();
            MealDays = new List<MealDays>(); 
        }
        public List<MealDays> MealDays { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        public string QRFID { get; set; }
    }
}
