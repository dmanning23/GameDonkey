using System;
using System.Collections.Generic;
using System.Text;

namespace GameDonkeyLib
{
	public class BaseObjectXML
	{
		public string model = "";
		public string animations = "";
		public List<string> garments = new List<string>();
		public List<StateContainerXML> states = new List<StateContainerXML>();
		public int height = 0;
	}
}
