using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameDonkey
{
	public class CreateHitCircleActionModel : CreateAttackActionModel
	{
		public float radius = 0.0f;
		public Vector2 startOffset = new Vector2(0.0f);
		public Vector2 velocity = new Vector2(0.0f);
	}
}
