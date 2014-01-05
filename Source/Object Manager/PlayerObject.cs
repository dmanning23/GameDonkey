using System;
using HadoukInput;
using GameTimer;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#if NETWORKING
using Microsoft.Xna.Framework.Net;
#endif
using FilenameBuddy;
using System.IO;
using System.Xml;

namespace GameDonkey
{
	/// <summary>
	/// this is a player game token, either human or AI
	/// </summary>
	public class PlayerObject : BaseObject
	{
		#region Members

		//the current angle of the animation rotation
		public float CurrentAnimationRotation { get; protected set; }

		//WEDDING STUFF

		/// <summary>
		/// the amount of time the player has been in the air.
		/// When this is over the TIME_TILL_FALL threshhold, the player's state machine is sent a "Fell" message
		/// </summary>
		public float AirDelta { get; protected set; }

		/// <summary>
		/// Number of times the player has been hit in a row
		/// </summary>
		private int m_ComboCounter;

		private const float m_fTimeToFall = 0.5f; //how long the character is in the air before they start to get "Fell" messages
		private const float m_fTimeForExtraGravity = 3.0f; //how long the character is in the air before they start to accumulate gravity
		private const float m_fHitPause = 0.2f; //how long hit pause is in this game
		private const float m_fStrengthMultiplier = 1.0f; //amount to multiply how far characters are hit in this game
		private const float m_fDamageMultiplier = 2.5f; //amount to multiply how much damage characters do in this game
		protected const float m_fNeutralFlyingSpeed = 900.0f; //how fast the character has to be moving through the air before they leave neutral state

		public string DeathSound { get; protected set; }

		/// <summary>
		/// texture to hold the portrait for the HUD
		/// </summary>
		protected Texture2D m_Portrait;

		/// <summary>
		/// the direction the thumbstick was held this frame
		/// </summary>
		protected Vector2 m_ThumbstickDirection;

		#endregion //Members

		#region Properties

		/// <summary>
		/// How much health this character has left
		/// </summary>
		public float Health { get; protected set; }

		public Texture2D Portrait
		{
			get { return m_Portrait; }
		}

		public int ComboCounter
		{
			get { return m_ComboCounter; }
		}

		#endregion //Properties

		#region Methods

		public PlayerObject(HitPauseClock rClock, int iQueueID)
			: base(EObjectType.Human, rClock, iQueueID)
		{
			//init is called by the base class, which will set everything up
		}

		public PlayerObject(EObjectType eType, HitPauseClock rClock, int iQueueID)
			: base(eType, rClock, iQueueID)
		{
			//init is called by the base class, which will set everything up
		}

		/// <summary>
		/// Constructor for replacing a network player when they leave the game
		/// </summary>
		/// <param name="rHuman">the dude to be replaced, copy all his shit</param>
		public PlayerObject(EObjectType eType, PlayerObject rHuman)
			: base(eType, rHuman)
		{
			//TODO: copy all the required shit
			AirDelta = rHuman.AirDelta;
			Health = rHuman.Health;
			m_Portrait = rHuman.m_Portrait;
			DeathSound = rHuman.DeathSound;
		}

		/// <summary>
		/// Replace all the base object pointers in this dude to point to a replacement object
		/// </summary>
		/// <param name="myBot">the replacement dude</param>
		public override void ReplaceOwner(PlayerObject myBot)
		{
			//replace in the state container
			States.ReplaceOwner(myBot);

			//replace in the physics
			m_Physics.ReplaceOwner(myBot);
		}

		protected override void Init()
		{
			CurrentAnimationRotation = 0.0f;
			m_ThumbstickDirection = Vector2.Zero;
			States = new PlayerObjectStateContainer();
			States.StateChangedEvent += this.StateChanged;
			m_Physics = new PlayerPhysicsContainer(this);

			Reset();
		}

		/// <summary>
		/// Reset the object for game start, character death
		/// </summary>
		public override void Reset()
		{
			base.Reset();
			Health = 10.0f;
			AirDelta = 0.0f;
			m_ComboCounter = 0;
		}

		public override int DisplayHealth()
		{
			return (int)(Health * 10.0f);
		}

		public override void Update(bool bUpdateGravity)
		{
			Debug.Assert(m_Velocity.X != float.NaN);
			Debug.Assert(m_Velocity.Y != float.NaN);
			Debug.Assert(m_Position.X != float.NaN);
			Debug.Assert(m_Position.Y != float.NaN);

			//update all our clocks
			Debug.Assert(null != CharacterClock);
			m_EvasionTimer.Update(CharacterClock);
			m_BlockTimer.Update(CharacterClock);
			m_TrailTimer.Update(CharacterClock);

			UpdateFallMessage(bUpdateGravity);

			//update the garments of this dude
			MyGarments.Update(CharacterClock);

			//update the state actions of this dude
			States.ExecuteActions(CharacterClock);

			UpdateEmitters();

			//update the animations
			UpdateRotation();

			m_AnimationContainer.Update(CharacterClock, m_Position, Flip, Scale, CurrentAnimationRotation, false);
		}

		/// <summary>
		/// update an input wrapper
		/// </summary>
		/// <param name="rController"></param>
		/// <param name="rInput"></param>
		public override void UpdateInput(InputWrapper rController, InputState rInput)
		{
			rController.Update(rInput, Flip);
		}

		/// <summary>
		/// Check if the character should be sent a "fall" message
		/// Overload this in your child class
		/// </summary>
		public virtual void UpdateFallMessage(bool bUpdateGravity)
		{
		}

		/// <summary>
		/// Update the character rotation before they are animated.
		/// Overload this function in the child class for your game
		/// </summary>
		public virtual void UpdateRotation()
		{
			//Overload in child classes!
		}

		/// <summary>
		/// Do all the specific processing to get player input.
		/// For human players, this means getting info from the controller.
		/// For AI players, this means reacting to info in the list of "bad guys"
		/// </summary>
		/// <param name="rController">the controller for this player (bullshit and ignored for AI)</param>
		/// <param name="listBadGuys">list of all the players (ignored for human players)</param>
		public override void GetPlayerInput(InputWrapper rController, List<PlayerQueue> listBadGuys)
		{
			//get the thumbstick direction
			m_ThumbstickDirection = rController.Controller.Thumbsticks.LeftThumbstick.Direction;

			//get the next moov from the input
			int iNextMoov = rController.GetNextMove();
			SendAttackMessage(iNextMoov);
		}

		/// <summary>
		/// fucntion to get the 'direction' this dude wants to go.
		/// for players this will be thumbstick direction
		/// for ai this will be direction they want to go
		/// for projectile this will be direction player thumbstick was held when rpojectile was shot
		/// </summary>
		/// <returns></returns>
		override public Vector2 Direction()
		{
			return m_ThumbstickDirection;
		}

		/// <summary>
		/// This is used to send attack moves from the input queue to the state machine, through the combo engine
		/// </summary>
		/// <param name="iMessage"></param>
		protected virtual void SendAttackMessage(int iNextMoov)
		{
			//am i currently in an attack state? 
			if (States.IsCurrentStateAttack())
			{
				//is the attack still currently active?
				if (States.IsAttackActive())
				{
					//wait until the attack ends

					if (-1 != iNextMoov)
					{
						//add the move to the queue
						m_QueuedInput.Enqueue(iNextMoov);
					}
				}

				//did that attack connect to a bad guy?
				else if (m_bAttackLanded)
				{
					//ok, the current attack is not active and there is queued input

					while (0 < m_QueuedInput.Count)
					{
						//send the queued input to the state machine!
						iNextMoov = m_QueuedInput.Dequeue();
						Debug.Assert(-1 != iNextMoov);
						SendStateMessage(iNextMoov);
					}
				}
			}
			//            else if (IsCancellableState(States.CurrentState()))
			//            {
			//                //The character is in a state where they cant attack, but we want to queue up for when this state ends

//                if (-1 != iNextMoov)
			//                {
			//#if DEBUG
			//                    string CurrentState = States.GetStateName(States.CurrentState());
			//                    string MoveName = States.GetMessageName(iNextMoov);
			//#endif

//                    //add the move to the queue
			//                    m_QueuedInput.Enqueue(iNextMoov);
			//                }
			//            }
			else
			{
				//ok character is in neutral state

				if (iNextMoov > -1)
				{
					//ok not in an attack state, no queued input
					SendStateMessage(iNextMoov);
				}
				else if (-1 == iNextMoov)
				{
					//clear out the queued input
					m_QueuedInput.Clear();
				}
			}
		}

		/// <summary>
		/// Check if the play is in a hard coded state,
		/// if so, check if its button is still being held down
		/// </summary>
		public override void CheckHardCodedStates()
		{
			base.CheckHardCodedStates();

			CheckMovementMessages();
		}

		/// <summary>
		/// Send any messages based on the direction the character is moving.
		/// </summary>
		public virtual void CheckMovementMessages()
		{
		}

		#region Hit Response

		public override void HitResponse(IGameDonkey rEngine)
		{
			Debug.Assert(null != m_Physics);

			//iterate through the hits, parsing as we go
			for (EHitType i = 0; i < EHitType.NumHits; i++)
			{
				if (Physics.HitFlags[(int)i])
				{
					switch (i)
					{
						case EHitType.AttackHit:
						{
							Debug.Assert(null != Physics.Hits[(int)i]);
							Debug.Assert(null != Physics.Hits[(int)i].Action);

							//is this a grab or an attack?
							if (EActionType.CreateThrow == Physics.Hits[(int)i].Action.ActionType)
							{
								//process grab hit
								RespondToGrab(Physics.Hits[(int)i]);
							}
							else
							{
								//process attack hit
								Debug.Assert((EActionType.CreateAttack == Physics.Hits[(int)i].Action.ActionType) ||
									(EActionType.CreateHitCircle == Physics.Hits[(int)i].Action.ActionType));
								RespondToAttack(Physics.Hits[(int)i], rEngine);
							}
						}
						break;

						case EHitType.GroundHit:
						case EHitType.CeilingHit:
						case EHitType.LeftWallHit:
						case EHitType.RightWallHit:
						{
							//taken care of in the base class
						}
						break;

						case EHitType.PushHit:
						{
							RespondToPushHit(Physics.Hits[(int)i]);
						}
						break;

						case EHitType.WeaponHit:
						{
							RespondToWeaponHit(Physics.Hits[(int)i], rEngine);
						}
						break;

						case EHitType.BlockHit:
						{
							RespondToBlockedAttack(Physics.Hits[(int)i], rEngine);
						}
						break;

						default:
						{
							//fuckass
							Debug.Assert(false);
						}
						break;
					}
				}
			}

			//Move the character
			if (null != CurrentThrow)
			{
				//did the other guy let us go?
				if (CurrentThrow.TimeToRelease <= CharacterClock.CurrentTime)
				{
					//send the 'done' message
					SendStateMessage((int)EMessage.Done);

					//set the velocity
					Vector2 throwVelocity = CurrentThrow.ReleaseDirection * m_fStrengthMultiplier;

					//flip the direction?
					if (!Flip)
					{
						throwVelocity.X *= -1.0f;
					}

					Velocity = throwVelocity;

					//null out that pointer
					m_rCurrentThrow = null;
				}
			}

			base.HitResponse(rEngine);
		}

		protected override void RespondToGroundHit(Hit rGroundHit, IGameDonkey rEngine)
		{
			Debug.Assert(null != m_Physics);
			Debug.Assert(EHitType.GroundHit == rGroundHit.HitType);

			//move the player UP out of the floor
			m_Position.Y += (rGroundHit.Strength * rGroundHit.Direction.Y);

			EState iCurrentState = (EState)States.CurrentState();
			if (EState.Stunned == iCurrentState)
			{
				//if the player is stunned, bounce them up in the air
				m_Velocity.Y = -1.0f * Math.Abs(m_Velocity.Y);

				rEngine.PlayParticleEffect(EDefaultParticleEffects.StunnedBounce,
					m_Velocity,
					rGroundHit.Position,
					Color.White);

				//add camera shake
				rEngine.AddCameraShake(0.2f);

				//TODO: make a sound for hitting boundary while stunned
			}

			if (0.0f < m_Velocity.Y)
			{
				//if the player's velocity is +y, it is set to 0
				AirDelta = 0.0f;
				m_Velocity.Y = 0.0f;

				Debug.Assert(m_Position.X != float.NaN);
				Debug.Assert(m_Position.Y != float.NaN);

				//WEDDING GAME

				//only send a ground hit message if the player was falling, 
				//this way the player won't "bounce" out of the floor when jumping through
				//SendStateMessage((int)EMessage.HitGround);
			}
		}

		protected override void RespondToCeilingHit(Hit rGroundHit, IGameDonkey rEngine)
		{
			Debug.Assert(null != m_Physics);
			Debug.Assert(EHitType.CeilingHit == rGroundHit.HitType);

			//move the player down out of the ceiling
			m_Position.Y += (rGroundHit.Strength * rGroundHit.Direction.Y);

			//always bounce the player out of a ceiling hit
			m_Velocity.Y = -1.0f * m_Velocity.Y;

			int iCurrentState = States.CurrentState();
			if ((int)EState.Stunned == iCurrentState)
			{
				rEngine.PlayParticleEffect(EDefaultParticleEffects.StunnedBounce,
					m_Velocity,
					rGroundHit.Position,
					Color.White);

				//add camera shake
				rEngine.AddCameraShake(0.2f);

				//TODO: make a sound for hitting boundary while stunned
			}
			else
			{
				//play the ceiling head bop particle effect
				rEngine.PlayParticleEffect(EDefaultParticleEffects.HeadBop,
					Vector2.Zero,
					rGroundHit.Position,
					Color.White);
			}

			//if the player's velocity is -y, it is set to 0
			if (m_Velocity.Y < 0.0f)
			{
				m_Velocity.Y = 0.0f;

				Debug.Assert(m_Position.X != float.NaN);
				Debug.Assert(m_Position.Y != float.NaN);
			}

			//TODO: make a particle effect & sound for hitting ceiling
		}

		protected override void RespondToLeftWallHit(Hit rGroundHit, IGameDonkey rEngine)
		{
			Debug.Assert(null != m_Physics);
			Debug.Assert(EHitType.LeftWallHit == rGroundHit.HitType);

			//move the player UP out of the floor
			m_Position.X += (rGroundHit.Strength * rGroundHit.Direction.X);

			EState iCurrentState = (EState)States.CurrentState();
			if (EState.Stunned == iCurrentState)
			{
				//if the player is stunned, bounce them up in the air
				m_Velocity.X = -1.0f * m_Velocity.X;

				rEngine.PlayParticleEffect(EDefaultParticleEffects.StunnedBounce,
					m_Velocity,
					rGroundHit.Position,
					Color.White);

				//add camera shake
				rEngine.AddCameraShake(0.2f);

				//TODO: make a sound for hitting boundary while stunned
			}

			//if the player's velocity is -X, it is set to 0
			if (m_Velocity.X < 0.0f)
			{
				m_Velocity.X = 0.0f;

				Debug.Assert(m_Position.X != float.NaN);
				Debug.Assert(m_Position.Y != float.NaN);
			}
		}

		protected override void RespondToRightWallHit(Hit rGroundHit, IGameDonkey rEngine)
		{
			Debug.Assert(null != m_Physics);
			Debug.Assert(EHitType.RightWallHit == rGroundHit.HitType);

			//move the player UP out of the floor
			m_Position.X += (rGroundHit.Strength * rGroundHit.Direction.X);

			EState iCurrentState = (EState)States.CurrentState();
			if (EState.Stunned == iCurrentState)
			{
				//if the player is stunned, bounce them up in the air
				m_Velocity.X = -1.0f * m_Velocity.X;

				rEngine.PlayParticleEffect(EDefaultParticleEffects.StunnedBounce,
					m_Velocity,
					rGroundHit.Position,
					Color.White);

				//add camera shake
				rEngine.AddCameraShake(0.2f);

				//TODO: make a sound for hitting boundary while stunned
			}

			//if the player's velocity is +X, it is set to 0
			if (0 < m_Velocity.X)
			{
				m_Velocity.X = 0.0f;

				Debug.Assert(m_Position.X != float.NaN);
				Debug.Assert(m_Position.Y != float.NaN);
			}
		}

		private void RespondToAttack(Hit rAttack, IGameDonkey rEngine)
		{
			Debug.Assert(EHitType.AttackHit == rAttack.HitType);
			Debug.Assert(null != rAttack);
			Debug.Assert(null != rAttack.Attacker);
			Debug.Assert(null != rAttack.Action);
			Debug.Assert(rAttack.Strength >= 0.0f);

			//set this dude's last attacker to the other dude
			m_rLastAttacker = rAttack.Attacker.PlayerQueue;
			Debug.Assert(null != m_rLastAttacker);

			if (IsBlocking())
			{
				//do a block!
				RespondToBlockedAttack(rAttack, rEngine);
			}
			else if (EvasionTimer.RemainingTime() <= 0.0f) //make sure the character is not evading
			{
				//if the player is already stunned, restart his state timer
				if (States.CurrentState() == (int)EState.Stunned)
				{
					States.ForceStateChange((int)EState.Stunned);
				}

				//add the damage
				Health -= (m_fDamageMultiplier * rAttack.Strength);

				//add the velocity
				Velocity = AttackedVector(rAttack);

				//send the state message
				SendStateMessage((int)EMessage.Hit);

				//do a hit pause
				CharacterClock.AddHitPause(m_fHitPause);
				rAttack.Attacker.CharacterClock.AddHitPause(m_fHitPause);

				//add camera shake
				rEngine.AddCameraShake(0.25f);

				//add the hit spark
				rEngine.PlayParticleEffect(EDefaultParticleEffects.HitSpark,
					Vector2.Zero,
					rAttack.Position,
					rAttack.Attacker.PlayerQueue.PlayerColor);

				//add a hit cloud
				rEngine.PlayParticleEffect(EDefaultParticleEffects.HitCloud,
					Vector2.Zero,
					rAttack.Position,
					Color.Yellow);

				//shoot particles out of teh characters butt
				rEngine.PlayParticleEffect(EDefaultParticleEffects.HitCloud,
					Velocity * 1.5f,
					rAttack.Position,
					Color.Yellow);

				//play the hit noise
				CreateAttackAction rAttackAction = rAttack.Action as CreateAttackAction;
				Debug.Assert(null != rAttackAction);
				rEngine.PlaySound(rAttackAction.HitSound);
			}

			//clear out the rest of the hits so that the player isn't hit multiple times by the same attack
			Physics.Reset();
		}

		private Vector2 AttackedVector(Hit rAttack)
		{
			Vector2 HitDirection = rAttack.Direction * m_fStrengthMultiplier;

			//if this player is already stunned, strengthen the hit
			if (States.CurrentState() == (int)EState.Stunned)
			{
				//This player was already stunned, increment the combo counter
				m_ComboCounter++;

				//add the combo multiplier to the hit direction
				float fMultiplier = 1.0f + (0.3f * m_ComboCounter);
				HitDirection *= fMultiplier;
			}
			else
			{
				//this player was not stunned, reset the combo counter
				m_ComboCounter = 0;
			}

			//add the attacking player's velocity to the hit direction
			HitDirection += (rAttack.Attacker.Velocity * 0.5f);

			return HitDirection;
		}

		/// <summary>
		/// I got grabbed by a bad guy
		/// </summary>
		/// <param name="rGrab">hit with all the grab info</param>
		private void RespondToGrab(Hit rGrab)
		{
			Debug.Assert(EHitType.AttackHit == rGrab.HitType);
			Debug.Assert(null != rGrab.Action);

			//TODO: does any grab logic need to be performed?
		}

		private void RespondToPushHit(Hit rPush)
		{
			Debug.Assert(EHitType.PushHit == rPush.HitType);

			//push away from all push hits!
			Vector2 deltaVect = rPush.Direction * rPush.Strength;
			deltaVect.Y = 0.0f;
			Position += deltaVect;
		}

		private void RespondToWeaponHit(Hit rWeaponHit, IGameDonkey rEngine)
		{
			Debug.Assert(EHitType.WeaponHit == rWeaponHit.HitType);
			Debug.Assert(null != rWeaponHit);
			Debug.Assert(null != rWeaponHit.Attacker);

			//set this dude's last attacker to the other dude
			m_rLastAttacker = rWeaponHit.Attacker.PlayerQueue;
			Debug.Assert(null != m_rLastAttacker);

			//if this player has over 100% damage, double the strength of the hit
			Vector2 HitDirection = AttackedVector(rWeaponHit) * 0.5f;

			//add the velocity
			Velocity = HitDirection;

			//do a hit pause
			CharacterClock.AddHitPause(m_fHitPause);
			rWeaponHit.Attacker.CharacterClock.AddHitPause(m_fHitPause);

			//add camera shake
			rEngine.AddCameraShake(0.08f);

			//do a special hit spark for weapon clash
			rEngine.PlayParticleEffect(EDefaultParticleEffects.WeaponHit,
					Velocity * 1.5f,
					rWeaponHit.Position,
					Color.White);
		}

		private void RespondToBlockedAttack(Hit rAttack, IGameDonkey rEngine)
		{
			//do a block!

			//add the velocity
			Velocity = AttackedVector(rAttack) * 0.9f;

			//do a hit pause
			CharacterClock.AddHitPause(m_fHitPause * 0.8f);
			rAttack.Attacker.CharacterClock.AddHitPause(m_fHitPause * 0.8f);

			//play the particle effect
			rEngine.PlayParticleEffect(EDefaultParticleEffects.Block,
				new Vector2((rAttack.Attacker.Flip ? -400.0f : 400.0f), 0.0f),
				rAttack.Position,
				new Color(0, 255, 255));

			//play the block noise
			rEngine.PlaySound("block");
		}

		#endregion //Hit Response

		protected override void Accelerate()
		{
			base.Accelerate();

			//Add extra gravity based on how long the character has been in the air
			if (AirDelta >= m_fTimeForExtraGravity)
			{
				//get how long past we are
				float fGravityAmount = (AirDelta - m_fTimeForExtraGravity) * 600.0f;

				//Get teh acceleration
				float myAcceleration = (fGravityAmount * CharacterClock.TimeDelta) * Scale;
				myAcceleration += Velocity.Y;

				//set the Y velocity?
				m_Velocity.Y = myAcceleration;
			}
		}

		#endregion //Methods

		#region Networking

#if NETWORKING

		/// <summary>
		/// Read this object from a network packet reader.
		/// </summary>
		public override void ReadFromNetwork(PacketReader packetReader)
		{
			//read in base object stuff
			base.ReadFromNetwork(packetReader);

			//read in player object specific stuff
			Health = packetReader.ReadSingle();

			////read in buttons
			//for (int i = 0; i < (int)EControllerAction.NumControllerActions; i++)
			//{
			//    m_bButtonPress[i] = packetReader.ReadBoolean();
			//}

			////read in directions
			//for (int i = 0; i < (int)EControllerAction.NumControllerActions; i++)
			//{
			//    m_bButtonHeld[i] = packetReader.ReadBoolean();
			//}
		}

		/// <summary>
		/// Write this object to a network packet reader.
		/// </summary>
		public override void WriteToNetwork(PacketWriter packetWriter)
		{
			//write out base object stuff
			base.WriteToNetwork(packetWriter);

			//write out player specific stuff
			packetWriter.Write(Health);

			////write out buttons
			//for (int i = 0; i < (int)EControllerAction.NumControllerActions; i++)
			//{
			//    packetWriter.Write(m_bButtonPress[i]);
			//}

			////write out directions
			//for (int i = 0; i < (int)EControllerAction.NumControllerActions; i++)
			//{
			//    packetWriter.Write(m_bButtonHeld[i]);
			//}
		}

#endif

		#endregion //Networking

		#region File IO

		/// <summary>
		/// Given an xml node, parse the contents.
		/// Override in child classes to read object-specific node types.
		/// </summary>
		/// <param name="childNode">the xml node to read</param>
		/// <param name="rEngine">the engine we are using to load</param>
		/// <param name="iMessageOffset">the message offset of this object's state machine</param>
		/// <returns></returns>
		public override bool ParseXmlNode(XmlNode childNode, IGameDonkey rEngine, int iMessageOffset)
		{
			//what is in this node?
			string strName = childNode.Name;
			string strValue = childNode.InnerXml;

			switch (strName)
			{
				case "stateMachine":
				{
					//ignored in players.
					return true;
				}

				case "states":
				{
					//ignored in players.
					return true;
				}

				case "portrait":
				{
					//get the portrait file
					Filename strPortraitFile = new Filename(strValue);
					//Debug.Assert(null != rEngine.Renderer.Content);
					//m_Portrait = rEngine.Renderer.Content.Load<Texture2D>(strPortraitFile.GetRelPathFileNoExt());
					return true;
				}

				case "deathSound":
				{
					DeathSound = childNode.InnerXml;
					//Debug.Assert(null != CAudioManager.GetCue(m_strDeathSound));
					return true;
				}

				case "GroundStates":
				{
					//get the ground states of this dude
					Filename groundStatesFile = new Filename(strValue);
					if (!States.ReadXmlStateContainer(new Filename(@"wedding state machines\ground state machine.xml"),
						iMessageOffset, 
						groundStatesFile, 
						this,
						rEngine, 
						true, 
						false))
					{
						Debug.Assert(false);
						return false;
					}
					return true;
				}

				case "UpStates":
				{
					//get teh upstates of this dude
					Filename upStatesFile = new Filename(childNode.InnerXml);
					if (!States.ReadXmlStateContainer(new Filename(@"wedding state machines\up state machine.xml"),
						iMessageOffset, 
						upStatesFile, 
						this, 
						rEngine, 
						true, 
						true))
					{
						Debug.Assert(false);
						return false;
					}
					return true;
				}

				case "DownStates":
				{
					////load the down states
					//strStatesFile.SetRelFilename(myCharXML.DownStates);
					//States.ReadStateContainer(rXmlContent, @"state machines\down state machine", iMessageOffset, strStatesFile.GetRelPathFileNoExt(), this, rEngine, true, true);
					//rXmlContent.Unload();
					return true;
				}

				case "ForwardStates":
				{
					////load the forward states
					//strStatesFile.SetRelFilename(myCharXML.ForwardStates);
					//States.ReadStateContainer(rXmlContent, @"state machines\forward state machine", iMessageOffset, strStatesFile.GetRelPathFileNoExt(), this, rEngine, true, true);
					//rXmlContent.Unload();
					return true;
				}

				default:
				{
					//punt to the base class
					return base.ParseXmlNode(childNode, rEngine, iMessageOffset);
				}
			}
		}

		public override bool LoadSerializedObject(ContentManager rXmlContent, Filename strResource, IGameDonkey rEngine, int iMessageOffset)
		{
			SPFSettings.PlayerObjectXML myCharXML = rXmlContent.Load<SPFSettings.PlayerObjectXML>(strResource.GetRelPathFileNoExt());

			//load the base object stuff
			Debug.Assert(null != PlayerQueue);

			Filename strModelFile = new Filename(myCharXML.model);
			Filename strAnimationFile = new Filename(myCharXML.animations);
			Filename strStateMachineFile = new Filename(myCharXML.stateMachine);
			m_fHeight = (float)myCharXML.height;

			//try to load the model
			if (!AnimationContainer.ReadSerializedModelFormat(rXmlContent, strModelFile, rEngine.Renderer))
			{
				Debug.Assert(false);
				return false;
			}
			rXmlContent.Unload();

			//load all the garments
			foreach (string strGarment in myCharXML.garments)
			{
				//get the garment filename
				Filename strGarmentFile = new Filename(strGarment);
				LoadSerializedGarment(rXmlContent, rEngine, strGarmentFile);
			}

			//read in the animations
			AnimationContainer.ReadSerializedAnimationFormat(rXmlContent, strAnimationFile);
			rXmlContent.Unload();

			//get the character portrait, load it from teh renderer content manager
			Filename strPortraitFile = new Filename(myCharXML.portrait);
			Debug.Assert(null != rEngine.Renderer.Content);
			m_Portrait = rEngine.Renderer.Content.Load<Texture2D>(strPortraitFile.GetRelPathFileNoExt());

			//grab the deathsound
			DeathSound = myCharXML.deathSound;
			//Debug.Assert(null != CAudioManager.GetCue(m_strDeathSound));

			//SPARROWHAWKS

			//load the ground states
			Filename strStatesFile = new Filename(myCharXML.GroundStates);
			States.ReadSerializedStateContainer(rXmlContent,
				new Filename(@"wedding state machines\ground state machine"), 
				iMessageOffset, 
				strStatesFile, 
				this, 
				rEngine, 
				true, 
				false);
			rXmlContent.Unload();

			//load the up states
			strStatesFile.SetRelFilename(myCharXML.UpStates);
			States.ReadSerializedStateContainer(rXmlContent,
				new Filename(@"wedding state machines\up state machine"), 
				iMessageOffset, 
				strStatesFile, 
				this, 
				rEngine, 
				true, 
				true);
			rXmlContent.Unload();

			////load the down states
			//strStatesFile.SetRelFilename(myCharXML.DownStates);
			//States.ReadStateContainer(rXmlContent, @"state machines\down state machine", iMessageOffset, strStatesFile.GetRelPathFileNoExt(), this, rEngine, true, true);
			//rXmlContent.Unload();

			////load the forward states
			//strStatesFile.SetRelFilename(myCharXML.ForwardStates);
			//States.ReadStateContainer(rXmlContent, @"state machines\forward state machine", iMessageOffset, strStatesFile.GetRelPathFileNoExt(), this, rEngine, true, true);
			//rXmlContent.Unload();

			return true;
		}

		#endregion //File IO
	}
}