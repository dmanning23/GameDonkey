using System;
using System.Collections.Generic;
using System.Text;

namespace GameDonkey
{
	public class BlockingStateActionModel : BaseActionModel
	{
		public float timeDelta = 0.0f;
		public string boneName = "";
		public string hitSound = "";
		public List<BaseActionModel> successActions = new List<BaseActionModel>();
	}
}
