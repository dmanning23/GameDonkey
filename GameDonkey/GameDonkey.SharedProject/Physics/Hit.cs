using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace GameDonkeyLib
{
	//The different types of hits
	public enum HitType
	{
		/// <summary>
		/// an object got attacked by another object
		/// </summary>
		Attack,

		/// <summary>
		/// an object's feet hit the ground
		/// </summary>
		Ground,

		/// <summary>
		/// an object hit another object and should be pushed away
		/// </summary>
		Push,

		/// <summary>
		/// an object's attacking weapon hit another object's attacking weapon
		/// </summary>
		Weapon,

		/// <summary>
		/// an object's attacking weapon hit another object's blocking weapon
		/// </summary>
		Block,

		/// <summary>
		/// an object hit the ceiling
		/// </summary>
		Ceiling,

		/// <summary>
		/// an object hit the left wall
		/// </summary>
		LeftWall,

		/// <summary>
		/// an object hit the right wall
		/// </summary>
		RightWall,
	};

	public class Hit
	{
		#region Properties

		/// <summary>
		/// The type of hit
		/// </summary>
		public HitType HitType { get; set; }

		/// <summary>
		/// teh guy that did the hitting
		/// </summary>
		public BaseObject Attacker { get; set; }

		/// <summary>
		/// if this is an attack action, this will point to the attack
		/// </summary>
		private CreateAttackAction AttackAction { get; set; }

		public float Strength { get; private set; }

		/// <summary>
		/// The direction of the hit
		/// </summary>
		public Vector2 Direction { get; set; }

		/// <summary>
		/// world coordinates of where the hit happened
		/// </summary>
		public Vector2 Position { get; set; }

		public bool IsThrow => (null != AttackAction && AttackAction.ActionType == EActionType.CreateThrow);

		public bool IsAoE => (null != AttackAction && AttackAction.AoE);

		/// <summary>
		/// flag for whether the hits are active this frame
		/// </summary>
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