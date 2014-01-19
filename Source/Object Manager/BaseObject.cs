using System;
using HadoukInput;
using GameTimer;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#if NETWORKING
using Microsoft.Xna.Framework.Net;
#endif
using StateMachineBuddy;
using ParticleBuddy;
using AnimationLib;
using DrawListBuddy;
using RenderBuddy;
using FilenameBuddy;
using CameraBuddy;

namespace GameDonkey
{
	/// <summary>
	/// All the different types of thing a base object can be
	/// </summary>
	public enum EObjectType
	{
		Human,
		AI,
		Level,
		Projectile
	}

	/// <summary>
	/// this is a game token, either player or projectile
	/// </summary>
	public class BaseObject
	{
		#region Members

		/// <summary>
		/// this is a counter for assigning round-robin item ids, this is the next id to use
		/// </summary>
		private static uint g_iIDCounter;

		/// <summary>
		/// the global id of this instance of a base object
		/// </summary>
		private uint m_iGlobalID;

		/// <summary>
		/// the id of this base object in teh player queue that owns it
		/// </summary>
		private int m_iQueueID;

		/// <summary>
		/// The type of thing this is
		/// </summary>
		private EObjectType m_eType;

		/// <summary>
		/// Reference to a clock that synchronizes all the different clocks in the dude
		/// </summary>
		public HitPauseClock CharacterClock { get; protected set; }

		/// <summary>
		/// the garment manager for this guy, used to make life easier
		/// </summary>
		public GarmentManager MyGarments { get; protected set; }

		/// <summary>
		/// List of this dude's currently active attacks
		/// </summary>
		public List<CreateAttackAction> CurrentAttacks { get; set; }

		/// <summary>
		/// If this dude is in a blocking state, this is a reference to that state
		/// </summary>
		public TimedActionList<BlockingStateAction> CurrentBlocks { get; set; }

		/// <summary>
		/// If this timer is running, it means attacks don't hit
		/// </summary>
		protected CountdownTimer m_BlockTimer;

		/// <summary>
		/// pointer to the current "throw" action (this dude is being thrown)
		/// </summary>
		protected CreateThrowAction m_rCurrentThrow;

		/// <summary>
		/// evasion timer, if this is running it means there are no push collsions
		/// </summary>
		protected CountdownTimer m_EvasionTimer;

		/// <summary>
		/// the animation container for this dude
		/// </summary>
		protected AnimationContainer m_AnimationContainer;

		/// <summary>
		/// this dude's position
		/// </summary>
		protected Vector2 m_Position;

		/// <summary>
		/// the velocity vector of this object
		/// </summary>
		protected Vector2 m_Velocity;

		/// <summary>
		/// When this timer runs out, 
		/// check the CharacterTrail object to see if we should drop another character image
		/// </summary>
		protected CountdownTimer m_TrailTimer;

		/// <summary>
		/// pointer to the current "character trail" object
		/// </summary>
		protected TrailAction m_rTrailAction;

		/// <summary>
		/// The player queue that owns this object
		/// </summary>
		protected PlayerQueue m_rPlayerQueue;

		/// <summary>
		/// The current color of this dude
		/// </summary>
		public Color PlayerColor { get; set; }

		/// <summary>
		/// Whether or not an attack has landed during the current state.  Used for combo engine.
		/// </summary>
		protected bool m_bAttackLanded;

		protected Queue<int> m_QueuedInput;

		/// <summary>
		/// How tall this character is (pixels)
		/// </summary>
		protected float m_fHeight;

		/// <summary>
		/// How big to draw, do physics at
		/// </summary>
		protected float m_fScale;

		/// <summary>
		/// drawlists used to draw the main character
		/// </summary>
		protected DrawList m_DrawList;

		/// <summary>
		/// Acceleration to apply to this dude for the current state
		/// </summary>
		protected ConstantAccelerationAction m_AccelAction;

		/// <summary>
		/// Decceleration to apply to this dude for the current state
		/// </summary>
		protected ConstantDeccelerationAction m_DeccelAction;

		/// <summary>
		/// The last player to attack this guy.  Used to calculate points when someone dies.
		/// </summary>
		protected PlayerQueue m_rLastAttacker;

		/// <summary>
		/// a list of all the current particle effect emitters launched from state actions
		/// Used to kill particle emitters when state changes
		/// </summary>
		private List<Emitter> m_listEmitters;

		#endregion //Members

		#region Properties

		public CountdownTimer EvasionTimer
		{
			get { return m_EvasionTimer; }
		}

		public CreateThrowAction CurrentThrow
		{
			get { return m_rCurrentThrow; }
			set { m_rCurrentThrow = value; }
		}

		public AnimationContainer AnimationContainer
		{
			get { return m_AnimationContainer; }
		}

		/// <summary>
		/// The state machine and state actions for this dude
		/// </summary>
		public IStateContainer States { get; set; }

		public Vector2 Position
		{
			get
			{
				Debug.Assert(m_Position.X != float.NaN);
				Debug.Assert(m_Position.Y != float.NaN);
				return m_Position;
			}
			set
			{
				m_Position = value;
				Debug.Assert(m_Position.X != float.NaN);
				Debug.Assert(m_Position.Y != float.NaN);
			}
		}

		/// <summary>
		/// this dude's orientation
		/// </summary>
		public bool Flip { get; set; }

		public Vector2 Velocity
		{
			get { return m_Velocity; }
			set { m_Velocity = value; }
		}

		public CountdownTimer TrailTimer
		{
			get { return m_TrailTimer; }
		}

		public TrailAction TrailAction
		{
			get { return m_rTrailAction; }
			set
			{
				m_rTrailAction = value;
				Debug.Assert(null != m_rTrailAction);
				m_TrailTimer.Start(m_rTrailAction.SpawnDelta);
			}
		}

		public PlayerQueue PlayerQueue
		{
			get { return m_rPlayerQueue; }
			set { m_rPlayerQueue = value; }
		}

		public uint GlobalID
		{
			get { return m_iGlobalID; }
		}

		public int QueueID
		{
			get { return m_iQueueID; }
		}

		/// <summary>
		/// thing for managing all the collisions, hits for this dude
		/// </summary>
		public IPhysicsContainer Physics { get; set; }

		public EObjectType Type
		{
			get { return m_eType; }
		}

		public float Height
		{
			get { return (m_fHeight * m_fScale); }
		}

		public float Scale
		{
			get { return m_fScale; }
			set
			{
				m_fScale = value;
				m_DrawList.Scale = m_fScale;
			}
		}

		/// <summary>
		/// Acceleration to apply to this dude for the current state
		/// </summary>
		public ConstantAccelerationAction AccelerationAction
		{
			get { return m_AccelAction; }
			set { m_AccelAction = value; }
		}

		/// <summary>
		/// Decceleration to apply to this dude for the current state
		/// </summary>
		public ConstantDeccelerationAction DeccelerationAction
		{
			get { return m_DeccelAction; }
			set { m_DeccelAction = value; }
		}

		public PlayerQueue LastAttacker
		{
			get { return m_rLastAttacker; }
		}

		public List<Emitter> Emitters
		{
			get { return m_listEmitters; }
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// initialize static member variables
		/// </summary>
		static BaseObject()
		{
			g_iIDCounter = 0;
		}

		/// <summary>
		/// hello, standard constructor!
		/// </summary>
		/// <param name="eType">the type of this object</param>
		/// <param name="rClock">a character clock.</param>
		public BaseObject(EObjectType eType, HitPauseClock rClock, int iQueueID)
		{
			m_eType = eType;
			m_iGlobalID = BaseObject.g_iIDCounter++;
			m_iQueueID = iQueueID;
			CurrentAttacks = new List<CreateAttackAction>();
			CurrentBlocks = new TimedActionList<BlockingStateAction>();
			m_BlockTimer = new CountdownTimer();
			m_EvasionTimer = new CountdownTimer();
			m_rCurrentThrow = null;
			m_AnimationContainer = new AnimationContainer();
			States = null;
			m_Position = new Vector2(0.0f);
			Flip = false;
			m_Velocity = new Vector2(0.0f);
			m_TrailTimer = new CountdownTimer();
			m_rTrailAction = null;
			m_rPlayerQueue = null;
			PlayerColor = Color.White;
			m_bAttackLanded = false;
			m_QueuedInput = new Queue<int>();
			m_fHeight = 0.0f;
			m_fScale = 1.0f;

			m_DrawList = new DrawList();
			m_DrawList.CurrentColor = Color.White;
			m_DrawList.Scale = m_fScale;

			m_AccelAction = null;
			m_DeccelAction = null;

			Debug.Assert(null != rClock);
			CharacterClock = rClock;

			m_rLastAttacker = null;

			MyGarments = new GarmentManager(this);
			m_listEmitters = new List<Emitter>();

			Init();

			Debug.Assert(null != Physics);
			Debug.Assert(null != States);
		}

		protected virtual void Init()
		{
			Physics = new PlayerPhysicsContainer(this);
			States = new ObjectStateContainer(new StateMachine());
			States.StateChangedEvent += this.StateChanged;
		}

		/// <summary>
		/// Constructor for replacing a network player when they leave the game
		/// </summary>
		/// <param name="rHuman">the dude to be replaced, copy all his shit</param>
		public BaseObject(EObjectType eType, BaseObject rHuman)
		{
			//grab all this shit
			m_eType = eType;
			m_iGlobalID = rHuman.m_iGlobalID;
			m_iQueueID = rHuman.m_iQueueID;
			CurrentAttacks = rHuman.CurrentAttacks;
			CurrentBlocks = rHuman.CurrentBlocks;
			m_BlockTimer = rHuman.m_BlockTimer;
			m_EvasionTimer = rHuman.m_EvasionTimer;
			m_rCurrentThrow = rHuman.m_rCurrentThrow;
			m_AnimationContainer = rHuman.m_AnimationContainer;
			if (null != States)
			{
				States.StateChangedEvent -= this.StateChanged;
			}
			States = rHuman.States;
			States.StateChangedEvent += this.StateChanged;
			m_Position = rHuman.m_Position;
			Flip = rHuman.Flip;
			m_Velocity = rHuman.m_Velocity;
			m_TrailTimer = rHuman.m_TrailTimer;
			m_rTrailAction = rHuman.m_rTrailAction;
			m_rPlayerQueue = rHuman.m_rPlayerQueue;
			PlayerColor = rHuman.PlayerColor;
			Physics = rHuman.Physics;
			m_bAttackLanded = rHuman.m_bAttackLanded;
			m_QueuedInput = rHuman.m_QueuedInput;
			m_fHeight = rHuman.m_fHeight;
			m_fScale = rHuman.m_fScale;
			m_DrawList = rHuman.m_DrawList;
			m_AccelAction = rHuman.m_AccelAction;
			m_DeccelAction = rHuman.m_DeccelAction;
			CharacterClock = rHuman.CharacterClock;
			m_rLastAttacker = rHuman.m_rLastAttacker;
			MyGarments = rHuman.MyGarments;
		}

		/// <summary>
		/// Replace all the base object pointers in this dude to point to a replacement object
		/// </summary>
		/// <param name="myBot">the replacement dude</param>
		public virtual void ReplaceOwner(PlayerObject myBot)
		{
			//should only be called in the child classes!
			Debug.Assert(false);
		}

		/// <summary>
		/// Reset the object for game start, character death
		/// </summary>
		public virtual void Reset()
		{
			Debug.Assert(null != CurrentAttacks);
			CurrentAttacks.Clear();
			CurrentBlocks.Reset();
			Debug.Assert(null != m_BlockTimer);
			m_BlockTimer.Stop();
			Debug.Assert(null != m_EvasionTimer);
			m_EvasionTimer.Stop();
			m_rCurrentThrow = null;
			Debug.Assert(null != States);
			States.Reset();
			m_Velocity = Vector2.Zero;
			Debug.Assert(null != m_TrailTimer);
			m_TrailTimer.Stop();
			m_rTrailAction = null;
			Debug.Assert(null != Physics);
			Physics.Reset();
			m_bAttackLanded = false;
			Debug.Assert(null != m_QueuedInput);
			m_QueuedInput.Clear();
			m_AccelAction = null;
			m_DeccelAction = null;
			m_rLastAttacker = null;
			MyGarments.Reset();
		}

		public virtual int DisplayHealth()
		{
			return 0;
		}

		public virtual void Update(bool bUpdateGravity)
		{
			//update all our clocks
			Debug.Assert(null != CharacterClock);
			m_EvasionTimer.Update(CharacterClock);
			m_BlockTimer.Update(CharacterClock);
			m_TrailTimer.Update(CharacterClock);

			//update the garments of this dude
			MyGarments.Update(CharacterClock);

			//update the state actions of this dude
			States.ExecuteActions(CharacterClock);

			UpdateEmitters();

			//update the animations
			m_AnimationContainer.Update(CharacterClock, m_Position, Flip, Scale, 0.0f, false);

			Debug.Assert(m_Position.X != float.NaN);
			Debug.Assert(m_Position.Y != float.NaN);
		}

		/// <summary>
		/// Clear out all the dead particle emitters
		/// </summary>
		protected void UpdateEmitters()
		{
			int i = 0;
			while (i < m_listEmitters.Count)
			{
				Emitter curEmitter = m_listEmitters[i];
				if (curEmitter.IsDead())
				{
					m_listEmitters.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
		}

		/// <summary>
		/// Do all the specific processing to get player input.
		/// For human players, this means getting info from the controller.
		/// For AI players, this means reacting to info in the list of "bad guys"
		/// </summary>
		/// <param name="rController">the controller for this player (bullshit and ignored for AI)</param>
		/// <param name="listBadGuys">list of all the players (ignored for human players)</param>
		public virtual void GetPlayerInput(InputWrapper rController, List<PlayerQueue> listBadGuys)
		{
		}

		/// <summary>
		/// update an input wrapper
		/// </summary>
		/// <param name="rController"></param>
		/// <param name="rInput"></param>
		public virtual void UpdateInput(InputWrapper rController, InputState rInput)
		{
		}

		public virtual void CheckHardCodedStates()
		{
			//TODO: move all this hardcode states junk into update

			//Apply acceleration to the character
			Accelerate();

			//apply decceleration to the character
			Deccelerate();
		}

		public void UpdateRagdoll(bool bIgnoreRagdoll)
		{
			m_AnimationContainer.UpdateRagdoll(bIgnoreRagdoll, Scale);
		}

		/// <summary>
		/// Add an attack to this dude's list of active attacks
		/// </summary>
		/// <param name="rAction">the attack action to perform</param>
		public void AddAttack(CreateAttackAction rAction)
		{
			Debug.Assert(null != CurrentAttacks);
			CurrentAttacks.Add(rAction);
		}

		/// <summary>
		/// set the block action of this object
		/// </summary>
		/// <param name="rAction">the block action to perform</param>
		public void AddBlock(CreateBlockAction rAction)
		{
			m_BlockTimer.Start(rAction.TimeDelta);
		}

		/// <summary>
		/// return the block state to false
		/// </summary>
		protected void ClearBlocks()
		{
			m_BlockTimer.Stop();
		}

		/// <summary>
		/// Send a message to this dudes state machine
		/// </summary>
		/// <param name="iMessage">the message to send to the state machine</param>
		/// <returns>bool: whether or not this dude changed states</returns>
		public void SendStateMessage(int iMessage)
		{
			//send the message to the state machine
			Debug.Assert(null != States);
			States.SendStateMessage(iMessage);
		}

		public void ForceStateChange(int iState)
		{
			//force the state change in the state machine
			Debug.Assert(null != States);
			States.ForceStateChange(iState);
		}

		/// <summary>
		/// Call this when the state changes to reset everything for the new state
		/// </summary>
		protected virtual void StateChanged(object sender, StateChangeEventArgs eventArgs)
		{
			//was this a turn around message?
			if (States.CurrentState() == (int)EState.TurningAround)
			{
				Flip = !Flip;
			}

			//clear the attacks
			CurrentAttacks.Clear();

			CurrentBlocks.Reset();

			//clear the blocks
			ClearBlocks();

			//clear the evades
			EvasionTimer.Stop();

			//clear the trail action
			m_TrailTimer.Stop();
			m_rTrailAction = null;

			//clear the accel & deccel
			m_AccelAction = null;
			m_DeccelAction = null;

			//remove any state specific garments
			MyGarments.Reset();

			//make sure to update this dude, 
			//because projectiles are activated in the player's update loop and placed in front of them in the update loop
			m_AnimationContainer.Update(CharacterClock, m_Position, Flip, Scale, 0.0f, false);

			m_bAttackLanded = false;

			//kill all the particle effects and clear out that list
			foreach (Emitter curEmitter in m_listEmitters)
			{
				curEmitter.EmitterTimer.Stop();
			}
			m_listEmitters.Clear();
		}

		public void CheckCollisions(BaseObject rBadGuy)
		{
			Debug.Assert(null != Physics);

			//make sure not to check collisions against ourselves
			if (GlobalID != rBadGuy.GlobalID)
			{
				Physics.CheckCollisions(rBadGuy.Physics);
			}
		}

		public void CheckWorldCollisions(Rectangle rWorldBoundaries)
		{
			Debug.Assert(null != Physics);
			Physics.CheckWorldCollisions(Velocity, rWorldBoundaries);
		}

		//public void CheckCollisions(List<CBullet> rBadGuys, Rectangle WorldBoundaries)
		//{
		//    for (int i = 0; i < rBadGuys.Count; i++)
		//    {
		//        Physics.CheckCollisions(rBadGuys[i]);
		//    }

		//    //check for world collisions
		//    Physics.CheckWorldCollisions(WorldBoundaries, Velocity);
		//}

		#region Collision Responses

		public virtual void CollisionResponse(IPhysicsContainer rOtherObject,
			CreateAttackAction rAttackAction,
			Vector2 FirstCollisionPoint,
			Vector2 SecondCollisionPoint)
		{
			Debug.Assert(null != rOtherObject);
			Debug.Assert(EObjectType.Level != rOtherObject.Owner.Type);
			Debug.Assert(null != rAttackAction);
			Debug.Assert(null != Physics);
			Debug.Assert(m_iGlobalID == rAttackAction.Owner.m_iGlobalID);

			//set "attack landed" flag for this state for combo engine
			BaseObject rPlayer = AttackLanded();
			Debug.Assert(null != rPlayer);
			Debug.Assert((EObjectType.AI == rPlayer.Type) || (EObjectType.Human == rPlayer.Type));

			if (!rOtherObject.HitFlags[(int)EHitType.AttackHit] || (rAttackAction.Strength > rOtherObject.Hits[(int)EHitType.AttackHit].Strength))
			{
				//i just punched the other object
				rOtherObject.HitFlags[(int)EHitType.AttackHit] = true;

				//am I facing left or right?
				Vector2 direction = rAttackAction.Direction;
				if (Flip)
				{
					direction.X *= -1.0f;
				}

				//the base object should be the player if this object is a projectile
				rOtherObject.Hits[(int)EHitType.AttackHit].Set(direction, rAttackAction, rAttackAction.Strength, EHitType.AttackHit, rPlayer, FirstCollisionPoint);

				//perform all the success actions
				if (rAttackAction.ExecuteSuccessActions(rOtherObject.Owner))
				{
					//if a state change occurred while the success actions were running, the attack list will be empty
					CurrentAttacks.Clear();
				}
			}
		}

		public virtual void WeaponCollisionResponse(IPhysicsContainer rOtherObject,
			CreateAttackAction rAttackAction,
			Vector2 FirstCollisionPoint,
			Vector2 SecondCollisionPoint)
		{
			Debug.Assert(null != rOtherObject);
			Debug.Assert(EObjectType.Level != rOtherObject.Owner.Type);
			Debug.Assert(null != rAttackAction);
			Debug.Assert(null != Physics);
			Debug.Assert(m_iGlobalID == rAttackAction.Owner.m_iGlobalID);

			//set "attack landed" flag for this state for combo engine
			BaseObject rPlayer = AttackLanded();
			Debug.Assert(null != rPlayer);
			Debug.Assert((EObjectType.AI == rPlayer.Type) || (EObjectType.Human == rPlayer.Type));

			//my weapon just collided with that other dude's weapon
			rOtherObject.HitFlags[(int)EHitType.WeaponHit] = true;

			//am I facing left or right?
			Vector2 direction = rAttackAction.Direction;
			if (Flip)
			{
				direction.X *= -1.0f;
			}

			//the base object should be the player if this object is a projectile
			rOtherObject.Hits[(int)EHitType.WeaponHit].Set(direction, rAttackAction, rAttackAction.Strength, EHitType.WeaponHit, rPlayer, FirstCollisionPoint);
		}

		/// <summary>
		/// i just attacked another dude but he blocked it
		/// </summary>
		/// <param name="rOtherObject"></param>
		/// <param name="rAttackAction"></param>
		/// <param name="FirstCollisionPoint"></param>
		/// <param name="SecondCollisionPoint"></param>
		public virtual void BlockResponse(IPhysicsContainer rOtherObject,
			CreateAttackAction rAttackAction,
			BlockingStateAction rOtherDudesAction,
			Vector2 FirstCollisionPoint,
			Vector2 SecondCollisionPoint)
		{
			Debug.Assert(null != rOtherObject);
			Debug.Assert(EObjectType.Level != rOtherObject.Owner.Type);
			Debug.Assert(null != rAttackAction);
			Debug.Assert(null != Physics);

			//that better be my attack
			Debug.Assert(m_iGlobalID == rAttackAction.Owner.m_iGlobalID);

			//that better be the other dude's block
			Debug.Assert(rOtherObject.Owner.m_iGlobalID == rOtherDudesAction.Owner.m_iGlobalID);

			//set "attack landed" flag for this state for combo engine
			BaseObject rPlayer = AttackLanded();
			Debug.Assert(null != rPlayer);
			Debug.Assert((EObjectType.AI == rPlayer.Type) || (EObjectType.Human == rPlayer.Type));

			if (!rOtherObject.HitFlags[(int)EHitType.BlockHit] || (rAttackAction.Strength > rOtherObject.Hits[(int)EHitType.BlockHit].Strength))
			{
				//i just punched the other object
				rOtherObject.HitFlags[(int)EHitType.BlockHit] = true;

				//am I facing left or right?
				Vector2 direction = rAttackAction.Direction;
				if (Flip)
				{
					direction.X *= -1.0f;
				}

				//the base object should be the player if this object is a projectile
				rOtherObject.Hits[(int)EHitType.BlockHit].Set(direction, rAttackAction, rAttackAction.Strength, EHitType.AttackHit, rPlayer, FirstCollisionPoint);

				//perform all the success actions for the BLOCKING action not the ATTACKING action!
				rOtherDudesAction.ExecuteSuccessActions();
			}
		}

		#endregion //Collision Responses

		public void RemoveAttack(int iAttackIndex)
		{
			Debug.Assert(iAttackIndex >= 0);
			if (iAttackIndex < CurrentAttacks.Count)
			{
				CurrentAttacks.RemoveAt(iAttackIndex);
			}
		}

		#region Hit Response

		public virtual void HitResponse(IGameDonkey rEngine)
		{
			Debug.Assert(null != Physics);

			//do boundary hits here in the base class
			if (Physics.HitFlags[(int)EHitType.GroundHit])
			{
				RespondToGroundHit(Physics.Hits[(int)EHitType.GroundHit], rEngine);
			}
			else if (Physics.HitFlags[(int)EHitType.CeilingHit])
			{
				RespondToCeilingHit(Physics.Hits[(int)EHitType.CeilingHit], rEngine);
			}

			if (Physics.HitFlags[(int)EHitType.LeftWallHit])
			{
				RespondToLeftWallHit(Physics.Hits[(int)EHitType.LeftWallHit], rEngine);
			}
			else if (Physics.HitFlags[(int)EHitType.RightWallHit])
			{
				RespondToRightWallHit(Physics.Hits[(int)EHitType.RightWallHit], rEngine);
			}

			//remove finished attacks from the list
			int iAttackIndex = 0;
			while (iAttackIndex < CurrentAttacks.Count)
			{
				if (CurrentAttacks[iAttackIndex].DoneTime <= CharacterClock.CurrentTime)
				{
					CurrentAttacks.RemoveAt(iAttackIndex);
				}
				else
				{
					iAttackIndex++;
				}
			}

			//remove finished blocks from list
			CurrentBlocks.Update(CharacterClock);

			if (null != CurrentThrow)
			{
				//okay, being thrown so don't add velocity
				m_Position = CurrentThrow.Bone.Position;
				Flip = !CurrentThrow.Owner.Flip;
			}
			else
			{
				//no throw, just add the velocity to the position
				m_Position += m_Velocity * CharacterClock.TimeDelta;
			}

			Physics.Reset();
		}

		protected virtual void RespondToGroundHit(Hit rGroundHit, IGameDonkey rEngine)
		{
			//TODO: override this in projectile and kill the projectile when it hits a wall

			//TOOD: override in level object and do nothing

			Debug.Assert(EHitType.GroundHit == rGroundHit.HitType);

			//move the player UP out of the floor
			m_Position.Y += (rGroundHit.Strength * rGroundHit.Direction.Y);

			//if the player's velocity is +y, it is set to 0
			if (0.0f < m_Velocity.Y)
			{
				m_Velocity.Y = 0.0f;

				Debug.Assert(m_Position.X != float.NaN);
				Debug.Assert(m_Position.Y != float.NaN);
			}
		}

		protected virtual void RespondToCeilingHit(Hit rGroundHit, IGameDonkey rEngine)
		{
			//TODO: override this in projectile and kill the projectile when it hits a wall

			//TOOD: override in level object and do nothing

			Debug.Assert(null != Physics);
			Debug.Assert(EHitType.CeilingHit == rGroundHit.HitType);

			//move the player down out of the ceiling
			m_Position.Y += (rGroundHit.Strength * rGroundHit.Direction.Y);

			//if the player's velocity is -y, it is set to 0
			if (0.0f > m_Velocity.Y)
			{
				m_Velocity.Y = 0.0f;

				Debug.Assert(m_Position.X != float.NaN);
				Debug.Assert(m_Position.Y != float.NaN);
			}
		}

		protected virtual void RespondToLeftWallHit(Hit rGroundHit, IGameDonkey rEngine)
		{
			//TODO: override this in projectile and kill the projectile when it hits a wall

			//TOOD: override in level object and do nothing

			Debug.Assert(null != Physics);
			Debug.Assert(EHitType.LeftWallHit == rGroundHit.HitType);

			//move the player UP out of the floor
			m_Position.X += (rGroundHit.Strength * rGroundHit.Direction.X);

			//if the player's velocity is -X, it is set to 0
			if (m_Velocity.X < 0.0f)
			{
				m_Velocity.X = 0.0f;

				Debug.Assert(m_Position.X != float.NaN);
				Debug.Assert(m_Position.Y != float.NaN);
			}
		}

		protected virtual void RespondToRightWallHit(Hit rGroundHit, IGameDonkey rEngine)
		{
			//TODO: override this in projectile and kill the projectile when it hits a wall

			//TOOD: override in level object and do nothing

			Debug.Assert(null != Physics);
			Debug.Assert(EHitType.RightWallHit == rGroundHit.HitType);

			//move the player UP out of the floor
			m_Position.X += (rGroundHit.Strength * rGroundHit.Direction.X);

			//if the player's velocity is +X, it is set to 0
			if (0 < m_Velocity.X)
			{
				m_Velocity.X = 0.0f;

				Debug.Assert(m_Position.X != float.NaN);
				Debug.Assert(m_Position.Y != float.NaN);
			}
		}

		#endregion //Hit Response

		/// <summary>
		/// add all the data for this dude to the camera
		/// </summary>
		/// <param name="rCamera"></param>
		public void AddToCamera(Camera rCamera)
		{
			//get half the height
			int iHalfHeight = (int)(m_fHeight * 0.68f);

			//add left/right points
			rCamera.AddPoint(new Vector2(Position.X - iHalfHeight, Position.Y));
			rCamera.AddPoint(new Vector2(Position.X + iHalfHeight, Position.Y));

			//add the bottom point
			rCamera.AddPoint(new Vector2(Position.X, Position.Y + (int)(m_fHeight * 0.65f)));

			//add the top
			rCamera.AddPoint(new Vector2(Position.X, Position.Y - (int)(m_fHeight * 0.77f)));
		}

		public virtual bool IsBlocking()
		{
			return (m_BlockTimer.RemainingTime() > 0.0f);
		}

		/// <summary>
		/// called when this object lands an attack on another object
		/// Set the attack landed flag in the owner character for the combo engine
		/// </summary>
		/// <returns>The player who landed the attack.</returns>
		public virtual BaseObject AttackLanded()
		{
			Debug.Assert(EObjectType.Projectile != Type);
			Debug.Assert(EObjectType.Level != Type);
			m_bAttackLanded = true;
			return this;
		}

		#region Rendering

		/// <summary>
		/// Check whether or not this dude should render a character trail
		/// </summary>
		/// <returns>bool: whether or not this dude needs to render a character trail</returns>
		public bool DoesNeedCharacterTrail()
		{
			//if there is no trail object, we definitly don't need this
			if (null != m_rTrailAction)
			{
				//check if the trail is still active
				if (CharacterClock.CurrentTime <= m_rTrailAction.DoneTime)
				{
					//check if the trail timer has expired
					if (m_TrailTimer.RemainingTime() <= 0.0f)
					{
						//eureka, we need a new trail!
						m_TrailTimer.Start(m_rTrailAction.SpawnDelta);
						return true;
					}
				}
				else
				{
					//if the trail is expired, set the pointer to 0 to save a cycle next time around
					m_rTrailAction = null;
				}
			}

			return false;
		}

		public virtual void UpdateDrawlist()
		{
			m_DrawList.Flush();
			AnimationContainer.Render(m_DrawList, PlayerColor);
		}

		public void Render(IRenderer rRenderer)
		{
			m_DrawList.Render(rRenderer);
		}

		public void RenderAttacks(IRenderer rRenderer)
		{
			for (int i = 0; i < CurrentAttacks.Count; i++)
			{
				if (null != CurrentAttacks[i].GetCircle())
				{
					CurrentAttacks[i].GetCircle().Render(rRenderer, Color.Red);
				}
			}

			for (int i = 0; i < CurrentBlocks.CurrentActions.Count; i++)
			{
				if (null != CurrentBlocks.CurrentActions[i].GetCircle())
				{
					CurrentBlocks.CurrentActions[i].GetCircle().Render(rRenderer, Color.Green);
				}
			}
		}

		public void RenderPhysics(IRenderer rRenderer)
		{
			m_AnimationContainer.Model.DrawPhysics(rRenderer, true, Color.White);
		}

		public void DrawCameraInfo(IRenderer rRenderer)
		{
			//get half the height
			int iHalfHeight = (int)(m_fHeight / 2.0f);

			//add left/right points
			rRenderer.Primitive.Point(new Vector2(Position.X - iHalfHeight, Position.Y), Color.Red);
			rRenderer.Primitive.Point(new Vector2(Position.X + iHalfHeight, Position.Y), Color.Red);

			//add the bottom point
			rRenderer.Primitive.Point(new Vector2(Position.X, Position.Y + (int)(m_fHeight * 0.55f)), Color.Red);

			//add the top
			rRenderer.Primitive.Point(new Vector2(Position.X, Position.Y - (int)(m_fHeight * 0.8f)), Color.Red);
		}

		#endregion //Rendering

		protected virtual void Accelerate()
		{
			//Is this character acclerating?
			if (null == m_AccelAction)
			{
				return;
			}

			//Get teh acceleration
			Vector2 myAcceleration = (m_AccelAction.GetMyVelocity() * CharacterClock.TimeDelta);
			myAcceleration += Velocity;

			//set the Y velocity?
			if (myAcceleration.Y > m_AccelAction.MaxVelocity.Y)
			{
				m_Velocity.Y = myAcceleration.Y;
			}

			//set the x velocity?
			float fAbsVelocity = Math.Abs(myAcceleration.X);
			if (fAbsVelocity < m_AccelAction.MaxVelocity.X)
			{
				m_Velocity.X = myAcceleration.X;
			}
		}

		protected void Deccelerate()
		{
			//Is this character decclerating?
			if (null == m_DeccelAction)
			{
				return;
			}

			//Get teh acceleration
			Vector2 myDecceleration = (m_DeccelAction.GetMyVelocity() * CharacterClock.TimeDelta);

			//set the y velocity
			myDecceleration.Y += Velocity.Y;
			if (myDecceleration.Y < m_DeccelAction.MinYVelocity)
			{
				m_Velocity.Y = myDecceleration.Y;
			}

			//set the X velocity
			if (m_Velocity.X <= 0.0f)
			{
				//moving left -x, flip the x decceleration
				myDecceleration.X *= -1.0f;
				myDecceleration.X += m_Velocity.X;

				//only deccelerate to 0
				m_Velocity.X = MathHelper.Clamp(myDecceleration.X, m_Velocity.X, 0.0f);
			}
			else
			{
				myDecceleration.X += m_Velocity.X;

				//only deccelerate to 0
				m_Velocity.X = MathHelper.Clamp(myDecceleration.X, 0.0f, m_Velocity.X);
			}
		}

		/// <summary>
		/// Get how far other objects need to stay away from this dude
		/// </summary>
		/// <returns>float, either part of the height or the distance to the edge of the nearest attack</returns>
		public float MinDistance()
		{
			if (CurrentAttacks.Count > 0)
			{
				//get teh distance to the nearest attack
				float fMinDistance = 0.0f;
				for (int i = 0; i < CurrentAttacks.Count; i++)
				{
					if (null != CurrentAttacks[i].GetCircle())
					{
						//get the distance along the x axis to the edge of the attack
						float fAttackDistance = CurrentAttacks[i].GetCircle().GetXDistance(Position);
						if ((fAttackDistance > fMinDistance) && (fAttackDistance != 0.0f))
						{
							fMinDistance = fAttackDistance;
						}
					}
				}

				//get the distance to the nearest block
				for (int i = 0; i < CurrentBlocks.CurrentActions.Count; i++)
				{
					if (null != CurrentAttacks[i].GetCircle())
					{
						//get the distance along the x axis to the edge of the attack
						float fAttackDistance = CurrentBlocks.CurrentActions[i].GetCircle().GetXDistance(Position);
						if ((fAttackDistance > fMinDistance) && (fAttackDistance != 0.0f))
						{
							fMinDistance = fAttackDistance;
						}
					}
				}

				return fMinDistance;
			}
			else
			{
				//no attacks, return the forward edge of the character
				return Height * 0.17f;
			}
		}

		/// <summary>
		/// Get how far other objects are before we dont care about them
		/// </summary>
		/// <returns>float, part of the height or the distance to the edge of the nearest attack</returns>
		public float MaxDistance()
		{
			float fMaxDistance = Height * 0.55f;

			//no attacks, return the forward edge of the character
			return fMaxDistance;
		}

		/// <summary>
		/// fucntion to get the 'direction' this dude wants to go.
		/// for players this will be thumbstick direction
		/// for ai this will be direction they want to go
		/// for projectile this will be direction player thumbstick was held when rpojectile was shot
		/// </summary>
		/// <returns></returns>
		virtual public Vector2 Direction()
		{
			return Vector2.Zero;
		}

		#region Tools

		/// <summary>
		/// Get a list of all the weapon
		/// </summary>
		/// <param name="listWeapons"></param>
		public void GetAllWeaponBones(List<string> listWeapons)
		{
			//get all the weapons from this dude's model
			m_AnimationContainer.Model.GetAllWeaponBones(listWeapons);

			//get all the weapons loaded into the garment manager
			MyGarments.GetAllWeaponBones(listWeapons);
		}

		#endregion //Tools

		#endregion //Methods

		#region Networking

#if NETWORKING

		/// <summary>
		/// Read this object from a network packet reader.
		/// </summary>
		public virtual void ReadFromNetwork(PacketReader packetReader)
		{
			//read in the timer
			CharacterClock.ReadFromNetwork(packetReader);

			//read in the animation stuff
			m_AnimationContainer.ReadFromNetwork(packetReader);

			//read in state machine stuff
			States.ReadFromNetwork(packetReader);

			//read in position stuff
			m_Position = packetReader.ReadVector2();
			m_bFlip = packetReader.ReadBoolean();
			m_Velocity = packetReader.ReadVector2();
		}

		/// <summary>
		/// Write this object to a network packet reader.
		/// </summary>
		public virtual void WriteToNetwork(PacketWriter packetWriter)
		{
			//write out the timer
			CharacterClock.WriteToNetwork(packetWriter);

			//read in the animation stuff
			m_AnimationContainer.WriteToNetwork(packetWriter);

			//read in state machine stuff
			States.WriteToNetwork(packetWriter);

			//read in position stuff
			packetWriter.Write(m_Position);
			packetWriter.Write(m_bFlip);
			packetWriter.Write(m_Velocity);
		}

#endif

		#endregion //Networking

		#region File IO

		/// <summary>
		/// Given an xml node, parse the contents.
		/// Override in child classes to read object-specific node types.
		/// </summary>
		/// <param name="childNode">the xml data to read</param>
		/// <param name="rEngine">the engine we are using to load</param>
		/// <param name="iMessageOffset">the message offset of this object's state machine</param>
		/// <returns></returns>
		public virtual bool ParseXmlData(BaseObjectData childNode, IGameDonkey rEngine, int iMessageOffset)
		{
			//read in the model
			if (!AnimationContainer.ReadXMLModelFormat(childNode.ModelFile, rEngine.Renderer))
			{
				Debug.Assert(false);
				return false;
			}
			Physics.SortBones(AnimationContainer.Model);

			//read in the animations
			if (!AnimationContainer.ReadXMLAnimationFormat(childNode.AnimationFile))
			{
				Debug.Assert(false);
				return false;
			}

			//read in the garments
			foreach (var garmentFile in childNode.GarmentFiles)
			{
				//Load up the garment.
				var myGarment = LoadXmlGarment(rEngine, garmentFile);
				Debug.Assert(null != myGarment);
			}

			//read in the states
			if (!States.ReadXmlStateContainer(childNode, rEngine, iMessageOffset, this))
			{
				return false;
			}

			//read in the height
			m_fHeight = childNode.Height;
			return true;
		}

		public Garment LoadXmlGarment(IGameDonkey rEngine, Filename strGarmentFile)
		{
			//load the garment
			Garment myGarment = new Garment();
			myGarment.ReadXMLFormat(strGarmentFile, rEngine.Renderer, AnimationContainer.Model);

			//add the garment to the dude
			myGarment.AddToModel();

			//sort all the bones in the physics engine
			Physics.SortBones(AnimationContainer.Model);
			Physics.GarmentChange(myGarment);

			return myGarment;
		}

		public virtual bool LoadSerializedObject(ContentManager rXmlContent, Filename strResource, IGameDonkey rEngine, int iMessageOffset)
		{
			SPFSettings.BaseObjectXML myCharXML = rXmlContent.Load<SPFSettings.BaseObjectXML>(strResource.GetRelPathFileNoExt());
			return LoadObject(rXmlContent, myCharXML, rEngine, iMessageOffset);
		}

		public virtual bool LoadObject(ContentManager rXmlContent, SPFSettings.BaseObjectXML myCharXML, IGameDonkey rEngine, int iMessageOffset)
		{
			Debug.Assert(null != PlayerQueue);

			Filename strModelFile = new Filename(myCharXML.model);
			Filename strAnimationFile = new Filename(myCharXML.animations);
			m_fHeight = (float)myCharXML.height;

			//load all the garments
			foreach (string strGarment in myCharXML.garments)
			{
				//get the garment filename
				Filename strGarmentFile = new Filename();
				strGarmentFile.SetRelFilename(strGarment);

				LoadSerializedGarment(rXmlContent, rEngine, strGarmentFile);
			}

			//try to load all that stuff
			if (!AnimationContainer.ReadSerializedModelFormat(rXmlContent, strModelFile, rEngine.Renderer))
			{
				Debug.Assert(false);
				return false;
			}
			Physics.SortBones(AnimationContainer.Model);
			AnimationContainer.ReadSerializedAnimationFormat(rXmlContent, strAnimationFile);

			//read in the state container
			States.ReadSerializedStateContainer(rXmlContent,
				myCharXML.states,
				rEngine,
				iMessageOffset,
				this);

			return true;
		}

		public Garment LoadSerializedGarment(ContentManager rXmlContent, IGameDonkey rEngine, Filename strGarmentFile)
		{
			//load the garment
			Garment myGarment = new Garment();
			myGarment.ReadSerializedFormat(rXmlContent, strGarmentFile, rEngine.Renderer, AnimationContainer.Model);

			//add the garment to the dude
			myGarment.AddToModel();
			rXmlContent.Unload();

			//sort all the bones in the physics engine
			Physics.SortBones(AnimationContainer.Model);
			Physics.GarmentChange(myGarment);

			return myGarment;
		}

		#endregion //File IO
	}
}