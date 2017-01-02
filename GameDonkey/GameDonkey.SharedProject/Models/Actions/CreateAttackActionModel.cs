using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameDonkey
{
	public class CreateAttackActionModel : BaseActionModel
	{
		public string boneName = "";
		public Vector2 direction = new Vector2(0.0f);
		public float damage = 0.0f;
		public float timeDelta = 0.0f;
		public string hitSound = "";
		public List<BaseActionModel> successActions = new List<BaseActionModel>();
	}
}
