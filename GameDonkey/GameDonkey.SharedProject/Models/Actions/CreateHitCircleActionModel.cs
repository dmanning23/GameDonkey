using MathNet.Numerics;
using Microsoft.Xna.Framework;
using System;
using System.Xml;
using Vector2Extensions;

namespace GameDonkeyLib
{
	public class CreateHitCircleActionModel : CreateAttackActionModel
	{
		#region Properties

		public override EActionType ActionType
		{
			get
			{
				return EActionType.CreateHitCircle;
			}
		}

		public float Radius { get; private set; }
		public Vector2 StartOffset { get; private set; }
		public Vector2 Velocity { get; private set; }

		#endregion //Properties

		#region Methods

		public CreateHitCircleActionModel()
		{
			StartOffset = Vector2.Zero;
			Velocity = Vector2.Zero;
		}

		#endregion //Methods
	}
}
