using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameDonkey
{
	public class ConstantDeccelerationActionModel : BaseActionModel
	{
		public Vector2 direction = new Vector2(0.0f);
		public float minYVelocity = 0.0f;
	}
}
