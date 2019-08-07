using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class SettingsSetReq
	{
		public SettingsSetReq()
		{
			Values = new List<Values>();
		}

		public string Type { get; set; }
		public List<Values> Values { get; set; }
		public string LoggedInUserContact_Id { get; set; }
	}
}
