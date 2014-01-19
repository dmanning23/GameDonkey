using GameTimer;
using StateMachineBuddy;
using System.Diagnostics;

namespace GameDonkey
{
	/// <summary>
	/// class for managing projectiles and how they interact with the player character
	/// </summary>
	class ProjectileObject : BaseObject
	{
		#region Members

		/// <summary>
		/// reference to the dude that owns this projectile
		/// </summary>
		BaseObject m_rPlayer;

		/// <summary>
		/// Whether or not this projectile can be blocked by other attacks
		/// </summary>
		public bool WeaponHits { get; private set; }

		#endregion //Members

		#region Methods

		public ProjectileObject(HitPauseClock rClock, BaseObject rPlayer, int iQueueID) : base(EObjectType.Projectile, rClock, iQueueID)
		{
			m_rPlayer = rPlayer;
		}

		protected override void Init()
		{
			Physics = new ProjectilePhysicsContainer(this);
			States = new ObjectStateContainer(new StateMachine());
			States.StateChangedEvent += this.StateChanged;
		}

		/// <summary>
		/// called when this object lands an attack on another object
		/// Set the attack landed flag in the owner character for the combo engine
		/// </summary>
		/// <returns>The player who landed the attack.</returns>
		public override BaseObject AttackLanded()
		{
			m_bAttackLanded = true;
			Debug.Assert(null != m_rPlayer);
			Debug.Assert((EObjectType.Human == m_rPlayer.Type) || (EObjectType.AI == m_rPlayer.Type));
			m_rPlayer.AttackLanded();
			return m_rPlayer;
		}

		/// <summary>
		/// Replace all the base object pointers in this dude to point to a replacement object
		/// </summary>
		/// <param name="myBot">the replacement dude</param>
		public override void ReplaceOwner(PlayerObject myBot)
		{
			m_rPlayer = myBot;
		}

		/// <summary>
		/// Given an xml node, parse the contents.
		/// Override in child classes to read object-specific node types.
		/// </summary>
		/// <param name="childNode">the xml data to read</param>
		/// <param name="rEngine">the engine we are using to load</param>
		/// <param name="iMessageOffset">the message offset of this object's state machine</param>
		/// <returns></returns>
		public override bool ParseXmlData(BaseObjectData childNode, IGameDonkey rEngine, int iMessageOffset)
		{
			ProjectileObjectData data = childNode as ProjectileObjectData;
			if (null == data)
			{
				return false;
			}

			WeaponHits = data.Weaponhits;
			return base.ParseXmlData(childNode, rEngine, iMessageOffset);
		}

		#endregion //Methods
	}
}