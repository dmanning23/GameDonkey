using Microsoft.Xna.Framework;
using ParticleBuddy;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameDonkey
{
	public class ParticleEffectActionModel : BaseActionModel
	{
		public string bone = "";
		public DirectionActionModel direction = new DirectionActionModel();
		public Vector2 StartOffset = new Vector2(0.0f);
		public bool UseBoneRotation = false;
		public List<ParticleXML> emitter = new List<ParticleXML>();
	}
}
