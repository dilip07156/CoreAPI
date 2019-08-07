using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class SettingsGetRes
	{
		public SettingsGetRes()
		{
			Values = new List<Values>();
			ResponseStatus = new ResponseStatus();
		}
		public List<Values> Values { get; set; }
		public ResponseStatus ResponseStatus { get; set; }
	}

    public class SettingsAutomatedGetRes
    {
        public SettingsAutomatedGetRes()
        { 
            ResponseStatus = new ResponseStatus();
        }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string UserEmailId { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}
