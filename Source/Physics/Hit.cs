using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace GameDonkey
{
	//The different types of hits
	public enum EHitType
	{
		AttackHit,	//an object got attacked by another object
		GroundHit,	//an object's feet hit the ground
		PushHit,	//an object hit another object and should be pushed away
		WeaponHit,	//an object's attacking weapon hit another object's attacking weapon
		BlockHit,	//an object's attacking weapon hit another object's blocking weapon
		CeilingHit,	//an object hit the ceiling
		LeftWallHit,	//an object hit the left wall
		RightWallHit,	//an object hit the right wall
		NumHits
	};

	public class Hit
	{
		#region Members

		//The direction of the hit
		private Vector2 m_Direction;

		//the strength of the hit
		private float m_fStrength;

		//The type of hit
		private EHitType m_eType;

		//teh guy that did the hitting
		private BaseObject m_rAttacker;

		/// <summary>
		/// if this is an attack action, this will point to the attack
		/// </summary>
		private IBaseAction m_rAction;

		/// <summary>
		/// world coordinates of where the hit happened
		/// </summary>
		private Vector2 m_Position;

		#endregion //Members

		#region Properties

		public EHitType HitType
		{
			get { return m_eType; }
		}

		public float Strength
		{
			get { return m_fStrength; }
		}

		public Vector2 Direction
		{
			get { return m_Direction; }
		}

		public IBaseAction Action
		{
			get { return m_rAction; }
		}

		public BaseObject Attacker
		{
			get { return m_rAttacker; }
		}

		public Vector2 Position
		{
			get { return m_Position; }
		}

		#endregion //Properties

		#region Methods

		public Hit()
		{
			m_Direction = Vector2.Zero;
			m_fStrength = 0.0f;
			m_eType = EHitType.AttackHit;
			m_rAction = null;
			m_rAttacker = null;
		}

		public void Set(Vector2 Direction, IBaseAction rAction, float fStrength, EHitType eType, BaseObject rAttacker, Vector2 rPosition)
		{
#if DEBUG
			if (EHitType.PushHit == eType)
			{
				float fLength = Direction.Length();
				Debug.Assert(Direction.Length() <= 2.0f);
			}
#endif

			m_Direction = Direction;
			m_fStrength = fStrength;
			m_eType = eType;
			m_rAttacker = rAttacker;
			m_rAction = rAction;
			m_Position = rPosition;
		}

		#endregion //Methods
	}
}