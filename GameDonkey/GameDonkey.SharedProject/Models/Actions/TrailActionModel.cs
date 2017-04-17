using System;
using System.Collections.Generic;
using System.Text;

namespace GameDonkeyLib
{
	public class TrailActionModel : BaseActionModel
	{
		public byte R = 0;
		public byte G = 0;
		public byte B = 0;
		public byte A = 0;
		public float lifeDelta = 0.0f;
		public float spawnDelta = 0.0f;
		public float timeDelta = 0.0f;
	}
}
