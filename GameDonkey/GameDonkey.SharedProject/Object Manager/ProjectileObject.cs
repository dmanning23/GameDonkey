using GameTimer;
using Microsoft.Xna.Framework.Content;
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
		BaseObject PlayerOwner;

		/// <summary>
		/// Whether or not this projectile can be blocked by other attacks
		/// </summary>
		public bool WeaponHits { get; private set; }

		#endregion //Members

		#region Methods

		public ProjectileObject(HitPauseClock clock, BaseObject playerOwner, int queueId) : base(EObjectType.Projectile, clock, queueId)
		{
			PlayerOwner = playerOwner;
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
			Debug.Assert(null != PlayerOwner);
			Debug.Assert((EObjectType.Human == PlayerOwner.Type) || (EObjectType.AI == PlayerOwner.Type));
			PlayerOwner.AttackLanded();
			return PlayerOwner;
		}

		/// <summary>
		/// Replace all the base object pointers in this dude to point to a replacement object
		/// </summary>
		/// <param name="myBot">the replacement dude</param>
		public override void ReplaceOwner(PlayerObject myBot)
		{
			PlayerOwner = myBot;
		}

		/// <summary>
		/// Given an xml node, parse the contents.
		/// Override in child classes to read object-specific node types.
		/// </summary>
		/// <param name="childNode">the xml data to read</param>
		/// <param name="engine">the engine we are using to load</param>
		/// <param name="messageOffset">the message offset of this object's state machine</param>
		/// <returns></returns>
		public override bool ParseXmlData(BaseObjectData childNode, IGameDonkey engine, int messageOffset, ContentManager content)
		{
			ProjectileObjectData data = childNode as ProjectileObjectData;
			if (null == data)
			{
				return false;
			}

			WeaponHits = data.Weaponhits;
			return base.ParseXmlData(childNode, engine, messageOffset, content);
		}

		#endregion //Methods
	}
}