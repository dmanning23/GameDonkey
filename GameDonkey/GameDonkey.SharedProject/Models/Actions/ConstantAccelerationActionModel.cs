using System;
using System.Collections.Generic;
using System.Text;

namespace GameDonkey
{
	public class ConstantAccelerationActionModel : BaseActionModel
	{
		public DirectionActionModel direction = new DirectionActionModel();
		public float maxVelocity = 0.0f;
	}
}
