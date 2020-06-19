using Microsoft.Xna.Framework;

namespace GameDonkeyLib
{
	//The different types of hits
	public enum EHitType
	{
		AttackHit,  //an object got attacked by another object
		GroundHit,  //an object's feet hit the ground
		PushHit,    //an object hit another object and should be pushed away
		WeaponHit,  //an object's attacking weapon hit another object's attacking weapon
		BlockHit,   //an object's attacking weapon hit another object's blocking weapon
		CeilingHit, //an object hit the ceiling
		LeftWallHit,    //an object hit the left wall
		RightWallHit,   //an object hit the right wall
		NumHits
	};

	public class Hit
	{
		#region Properties

		//The direction of the hit
		private Vector2 _direction;

		//The type of hit
		public EHitType HitType { get; private set; }

		//teh guy that did the hitting
		public BaseObject Attacker { get; private set; }

		/// <summary>
		/// if this is an attack action, this will point to the attack
		/// </summary>
		public CreateAttackAction AttackAction { get; private set; }

		/// <summary>
		/// world coordinates of where the hit happened
		/// </summary>
		private Vector2 _position;

		public float Strength { get; private set; }

		public Vector2 Direction
		{
			get { return _direction; }
		}

		public Vector2 Position
		{
			get { return _position; }
		}

		#endregion //Properties

		#region Methods

		public Hit()
		{
			_direction = Vector2.Zero;
			Strength = 0.0f;
			HitType = EHitType.AttackHit;
			AttackAction = null;
			Attacker = null;
		}

		public void Set(Vector2 direction, CreateAttackAction attackAction, float strength, EHitType hitType, BaseObject attacker, Vector2 position)
		{
			_direction = direction;
			Strength = strength;
			HitType = hitType;
			Attacker = attacker;
			AttackAction = attackAction;
			_position = position;
		}

		#endregion //Methods
	}
}