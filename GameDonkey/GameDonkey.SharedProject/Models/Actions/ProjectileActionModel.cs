using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameDonkey
{
	public class ProjectileActionModel : BaseActionModel
	{
		public string filename = "";
		public Vector2 startOffset = new Vector2(0.0f);
		public float scale = 0.0f;
		public DirectionActionModel direction = new DirectionActionModel();
	}
}
