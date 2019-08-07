using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class SettingsSetRes
	{
		public SettingsSetRes()
		{
			ResponseStatus = new ResponseStatus();
		}
		public ResponseStatus ResponseStatus { get; set; }
	}
}
