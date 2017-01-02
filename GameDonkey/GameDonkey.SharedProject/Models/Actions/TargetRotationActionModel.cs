using System;
using System.Collections.Generic;
using System.Text;

namespace GameDonkey
{
	public class TargetRotationActionModel : BaseActionModel
	{
		public float timeDelta = 0.0f;
		public DirectionActionModel targetRotation = new DirectionActionModel();
	}
}
