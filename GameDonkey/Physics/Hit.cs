using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace GameDonkeyLib
{
	//The different types of hits
	public enum HitType
	{
		Attack,
		Ground,
		Push,
		Weapon,
		Block,
		Ceiling,
		LeftWall,
		RightWall,
	};

	public class Hit
	{
		#region Properties
		public HitType HitType { get; set; }
		public BaseObject Attacker { get; set; }
		private CreateAttackAction AttackAction { get; set; }

		public float Strength { get; private set; }
		public Vector2 Direction { get; set; }
		public Vector2 Position { get; set; }

		public bool IsThrow => (null != AttackAction && AttackAction.ActionType == EActionType.CreateThrow);

		public bool IsAoE => (null != AttackAction && AttackAction.AoE);
		public bool Active { get; set; } = false;

		public SoundEffect HitSound => (null != AttackAction) ? AttackAction.HitSound : null;

		#endregion //Properties

		#region Methods
		public Hit(Vector2 direction, CreateAttackAction attackAction, float strength, HitType hitType, BaseObject attacker, Vector2 position)
		{
			Set(direction, attackAction, strength, hitType, attacker, position);
		}

		public Hit() : this(Vector2.Zero, null, 0f, HitType.Attack, null, Vector2.Zero)
		{
			Active = false;
		}

		public void Set(Vector2 direction, CreateAttackAction attackAction, float strength, HitType hitType, BaseObject attacker, Vector2 position)
		{
			Active = true;
			Direction = direction;
			Strength = strength;
			HitType = hitType;
			Attacker = attacker;
			AttackAction = attackAction;
			Position = position;
		}

		#endregion //Methods
	}
}