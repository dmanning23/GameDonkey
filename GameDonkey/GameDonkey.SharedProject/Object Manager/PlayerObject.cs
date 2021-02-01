using GameTimer;
using HadoukInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameDonkeyLib
{
	/// <summary>
	/// this is a player game token, either human or AI
	/// </summary>
	public abstract class PlayerObject : BaseObject
	{
		#region Properties

		/// <summary>
		/// Number of times the player has been hit in a row
		/// </summary>
		public int ComboCounter { get; private set; }

		/// <summary>
		/// how long hit pause is in this game
		/// </summary>
		protected virtual float HitPause => 0.2f;

		/// <summary>
		/// texture to hold the portrait for the HUD
		/// </summary>
		public Texture2D Portrait { get; protected set; }

		/// <summary>
		/// the direction the thumbstick was held this frame
		/// </summary>
		protected Vector2 ThumbstickDirection;

		public abstract float Health { get; set; }

		public event EventHandler<HealthEventArgs> HealthChangedEvent;

		public AIController AI { get; set; }

		#endregion //Properties

		#region Methods

		public PlayerObject(HitPauseClock clock, int queueId, string name)
			: base(GameObjectType.Human, clock, queueId, name)
		{
			//init is called by the base class, which will set everything up
		}

		public PlayerObject(GameObjectType gameObjectType, HitPauseClock clock, int queueId, string name)
			: base(gameObjectType, clock, queueId, name)
		{
			//init is called by the base class, which will set everything up
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
			Physics.ReplaceOwner(myBot);
		}

		protected abstract IStateContainer CreateStateContainer();

		protected override void Init()
		{
			ThumbstickDirection = Vector2.Zero;
			States = CreateStateContainer();
			States.StateChangedEvent += this.StateChanged;
			Physics = new PlayerPhysicsContainer(this);

			Reset();
		}

		/// <summary>
		/// Reset the object for game start, character death
		/// </summary>
		public override void Reset()
		{
			base.Reset();
			ResetHealth();
		}

		public override void Update()
		{
			//update all our clocks
			EvasionTimer.Update(CharacterClock);
			TrailTimer.Update(CharacterClock);

			UpdateFallMessage();

			//update the garments of this dude
			Garments.Update(CharacterClock);

			//update the state actions of this dude
			States.ExecuteActions(CharacterClock);

			UpdateEmitters();

			//update the animations
			UpdateRotation();

			UpdateAnimation();
		}

		/// <summary>
		/// update an input wrapper
		/// </summary>
		/// <param name="controller"></param>
		/// <param name="input"></param>
		public override void UpdateInput(InputWrapper controller, IInputState input)
		{
			controller.Update(input, Flip);
		}

		/// <summary>
		/// Check if the character should be sent a "fall" message
		/// Overload this in your child class
		/// </summary>
		public virtual void UpdateFallMessage()
		{
			//Overload in child classes!
		}

		/// <summary>
		/// Update the character rotation before they are animated.
		/// Overload this function in the child class for your game
		/// </summary>
		public virtual void UpdateRotation()
		{
			//Overload in child classes!
		}

		public override void GetPlayerInput(InputWrapper controller, List<PlayerQueue> listBadGuys, bool ignoreAttackInput)
		{
			if (null != AI)
			{
				AI.Update();
				AI.GetPlayerInput(listBadGuys, ignoreAttackInput);
				ThumbstickDirection = AI.Direction;
			}
			else
			{
				//get the thumbstick direction
				ThumbstickDirection = controller.Controller.Thumbsticks.LeftThumbstick.Direction;

				//get the next moov from the input
				var nextMoov = controller.GetNextMove();
				if (!ignoreAttackInput)
				{
					SendAttackMessage(nextMoov);
				}
				else
				{
					//if we are ignoring attack input, only send the message if it isn't an attack
					if (!States.IsAttackMessage(nextMoov))
					{
						SendAttackMessage(nextMoov);
					}
				}
			}
		}

		/// <summary>
		/// fucntion to get the 'direction' this dude wants to go.
		/// for players this will be thumbstick direction
		/// for ai this will be direction they want to go
		/// for projectile this will be direction player thumbstick was held when rpojectile was shot
		/// </summary>
		/// <returns></returns>
		public override Vector2 Direction()
		{
			return new Vector2(ThumbstickDirection.X, ThumbstickDirection.Y * -1f);
		}

		/// <summary>
		/// This is used to send attack moves from the input queue to the state machine, through the combo engine
		/// </summary>
		/// <param name="iMessage"></param>
		protected virtual void SendAttackMessage(string nextMoov)
		{
			//am i currently in an attack state? 
			if (States.IsCurrentStateAttack())
			{
				//is the attack still currently active?
				if (States.IsAttackActive())
				{
					//wait until the attack ends

					if (!string.IsNullOrEmpty(nextMoov))
					{
						//add the move to the queue
						_queuedInput.Enqueue(nextMoov);
					}
				}

				//did that attack connect to a bad guy?
				else if (_attackLanded)
				{
					//ok, the current attack is not active and there is queued input

					while (0 < _queuedInput.Count)
					{
						//send the queued input to the state machine!
						nextMoov = _queuedInput.Dequeue();
						SendStateMessage(nextMoov);
					}
				}
			}
			else
			{
				//ok character is in neutral state

				if (!string.IsNullOrEmpty(nextMoov))
				{
					//ok not in an attack state, no queued input
					SendStateMessage(nextMoov);
				}
				else
				{
					//clear out the queued input
					_queuedInput.Clear();
				}
			}
		}

		#region Hit Response

		public override void HitResponse(IGameDonkey engine)
		{
			//iterate through the hits, parsing as we go
			for (EHitType i = 0; i < EHitType.NumHits; i++)
			{
				if (Physics.HitFlags[(int)i])
				{
					switch (i)
					{
						case EHitType.AttackHit:
							{
								//is this a grab or an attack?
								if (EActionType.CreateThrow == Physics.Hits[(int)i].AttackAction.ActionType)
								{
									//process grab hit
									RespondToGrab(Physics.Hits[(int)i]);
								}
								else
								{
									//process attack hit
									RespondToAttack(Physics.Hits[(int)i], engine);
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
								RespondToWeaponHit(Physics.Hits[(int)i], engine);
							}
							break;

						case EHitType.BlockHit:
							{
								RespondToBlockedAttack(Physics.Hits[(int)i], engine);
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
					SendStateMessage("Done");

					//set the velocity
					var throwVelocity = CurrentThrow.Direction;

					//flip the direction?
					if (!Flip)
					{
						throwVelocity.X *= -1.0f;
					}

					Velocity = throwVelocity;

					//null out that pointer
					CurrentThrow = null;
				}
			}

			base.HitResponse(engine);
		}

		protected override void RespondToGroundHit(Hit groundHit, IGameDonkey engine)
		{
			//move the player UP out of the floor
			_position.Y += (groundHit.Strength * groundHit.Direction.Y);

			if (States.CurrentState == "Stunned")
			{
				//if the player is stunned, bounce them up in the air
				_velocity.Y = -1.0f * Math.Abs(Velocity.Y);

				engine.PlayParticleEffect(DefaultParticleEffect.StunnedBounce,
					Velocity,
					groundHit.Position,
					Color.White);

				//add camera shake
				engine.AddCameraShake(0.2f);

				//TODO: make a sound for hitting boundary while stunned
			}

			if (0.0f < Velocity.Y)
			{
				//if the player's velocity is +y, it is set to 0
				_velocity.Y = 0.0f;
			}
		}

		protected override void RespondToCeilingHit(Hit groundHit, IGameDonkey engine)
		{
			//move the player down out of the ceiling
			_position.Y += (groundHit.Strength * groundHit.Direction.Y);

			//always bounce the player out of a ceiling hit
			_velocity.Y = -1.0f * Velocity.Y;

			if (States.CurrentState == "Stunned")
			{
				//add camera shake
				engine.AddCameraShake(0.2f);
			}

			//if the player's velocity is -y, it is set to 0
			if (Velocity.Y < 0.0f)
			{
				_velocity.Y = 0.0f;
			}
		}

		protected override void RespondToLeftWallHit(Hit groundHit, IGameDonkey engine)
		{
			//move the player UP out of the floor
			_position.X += (groundHit.Strength * groundHit.Direction.X);

			if (States.CurrentState == "Stunned")
			{
				//if the player is stunned, bounce them up in the air
				_velocity.X = -1.0f * Velocity.X;

				engine.PlayParticleEffect(DefaultParticleEffect.StunnedBounce,
					Velocity,
					groundHit.Position,
					Color.White);

				//add camera shake
				engine.AddCameraShake(0.2f);

				//TODO: make a sound for hitting boundary while stunned
			}

			//if the player's velocity is -X, it is set to 0
			if (Velocity.X < 0.0f)
			{
				_velocity.X = 0.0f;
			}
		}

		protected override void RespondToRightWallHit(Hit groundHit, IGameDonkey engine)
		{
			//move the player UP out of the floor
			_position.X += (groundHit.Strength * groundHit.Direction.X);

			if (States.CurrentState == "Stunned")
			{
				//if the player is stunned, bounce them up in the air
				_velocity.X = -1.0f * Velocity.X;

				engine.PlayParticleEffect(DefaultParticleEffect.StunnedBounce,
					Velocity,
					groundHit.Position,
					Color.White);

				//add camera shake
				engine.AddCameraShake(0.2f);

				//TODO: make a sound for hitting boundary while stunned
			}

			//if the player's velocity is +X, it is set to 0
			if (0 < Velocity.X)
			{
				_velocity.X = 0.0f;
			}
		}

		protected virtual void RespondToAttack(Hit attack, IGameDonkey engine)
		{
			//set this dude's last attacker to the other dude
			LastAttacker = attack.Attacker.PlayerQueue;

			if (IsShielded())
			{
				//do a block!
				ShieldActions.CurrentActions[0].ExecuteSuccessActions();
				RespondToBlockedAttack(attack, engine);
			}
			else if (EvasionTimer.RemainingTime <= 0.0f) //make sure the character is not evading
			{
				//if the player is already stunned, restart his state timer
				if (States.CurrentState == "Stunned")
				{
					States.ForceStateChange("Stunned");
				}

				//add the damage
				HealthChanged(attack.Strength);
				TakeDamage(attack.Strength);

				//add the velocity
				Velocity = AttackedVector(attack);

				//send the state message
				SendStateMessage("Hit");

				//do a hit pause
				if (!attack.AttackAction.AoE)
				{
					CharacterClock.AddHitPause(HitPause);
					attack.Attacker.CharacterClock.AddHitPause(HitPause);
				}

				//add camera shake
				engine.AddCameraShake(0.25f);

				//add the hit spark
				engine.PlayParticleEffect(DefaultParticleEffect.HitSpark,
					Vector2.Zero,
					attack.Position,
					attack.Attacker.PlayerQueue.PlayerColor);

				//add a hit cloud
				engine.PlayParticleEffect(DefaultParticleEffect.HitCloud,
					Vector2.Zero,
					attack.Position,
					Color.Yellow);

				//shoot particles out of teh characters butt
				engine.PlayParticleEffect(DefaultParticleEffect.HitCloud,
					Velocity * 1.5f,
					attack.Position,
					Color.Yellow);

				//play the hit noise
				var attackAction = attack.AttackAction as CreateAttackAction;

				if (null != attackAction.HitSound)
				{
					attackAction.HitSound.Play();
				}
			}

			//clear out the rest of the hits so that the player isn't hit multiple times by the same attack
			Physics.Reset();
		}

		protected void HealthChanged(float attackStrength)
		{
			HealthChangedEvent?.Invoke(this, new HealthEventArgs(attackStrength));
		}

		private Vector2 AttackedVector(Hit attack)
		{
			var HitDirection = attack.Direction;

			//if this player is already stunned, strengthen the hit
			if (States.CurrentState == "Stunned")
			{
				//This player was already stunned, increment the combo counter
				ComboCounter++;

				//add the combo multiplier to the hit direction
				float multiplier = 1.0f + (0.3f * ComboCounter);
				HitDirection *= multiplier;
			}
			else
			{
				//this player was not stunned, reset the combo counter
				ComboCounter = 0;
			}

			//add the attacking player's velocity to the hit direction
			HitDirection += (attack.Attacker.Velocity * 0.5f);

			return HitDirection;
		}

		/// <summary>
		/// I got grabbed by a bad guy
		/// </summary>
		/// <param name="rGrab">hit with all the grab info</param>
		private void RespondToGrab(Hit grab)
		{
			//TODO: does any grab logic need to be performed?
		}

		protected virtual void RespondToPushHit(Hit push)
		{
			//push away from all push hits!
			var deltaVect = push.Direction * push.Strength;
			deltaVect.Y = 0.0f;
			Position += deltaVect;
		}

		protected virtual void RespondToWeaponHit(Hit weaponHit, IGameDonkey engine)
		{
			//set this dude's last attacker to the other dude
			LastAttacker = weaponHit.Attacker.PlayerQueue;

			//if this player has over 100% damage, double the strength of the hit
			var hitDirection = AttackedVector(weaponHit) * 0.5f;

			//add the velocity
			Velocity = hitDirection;

			//do a hit pause
			if (!weaponHit.AttackAction.AoE)
			{
				CharacterClock.AddHitPause(HitPause);
				weaponHit.Attacker.CharacterClock.AddHitPause(HitPause);
			}

			//add camera shake
			engine.AddCameraShake(0.08f);

			//do a special hit spark for weapon clash
			engine.PlayParticleEffect(DefaultParticleEffect.WeaponHit,
					Velocity * 1.5f,
					weaponHit.Position,
					Color.White);
		}

		protected virtual void RespondToBlockedAttack(Hit attack, IGameDonkey engine)
		{
			//do a block!

			//add the velocity
			Velocity = AttackedVector(attack) * 0.9f;

			//do a hit pause
			if (!attack.AttackAction.AoE)
			{
				CharacterClock.AddHitPause(HitPause * 0.8f);
				attack.Attacker.CharacterClock.AddHitPause(HitPause * 0.8f);
			}

			//play the particle effect
			engine.PlayParticleEffect(DefaultParticleEffect.Block,
				new Vector2((attack.Attacker.Flip ? -400.0f : 400.0f), 0.0f),
				attack.Position,
				new Color(0, 255, 255));
		}

		#endregion //Hit Response

		public abstract void ResetHealth();

		public abstract bool CheckIfDead();

		public abstract void TakeDamage(float damage);

		#endregion //Methods

		#region File IO

		/// <summary>
		/// Given an xml node, parse the contents.
		/// Override in child classes to read object-specific node types.
		/// </summary>
		/// <param name="childNode">the xml data to read</param>
		/// <param name="engine">the engine we are using to load</param>
		/// <param name="messageOffset">the message offset of this object's state machine</param>
		/// <returns></returns>
		public override void ParseXmlData(BaseObjectModel model, IGameDonkey engine, ContentManager content)
		{
			PlayerObjectModel data = model as PlayerObjectModel;
			if (null == data)
			{
				throw new Exception("must pass PlayerObjectModel to PlayerObject.ParseXmlData");
			}

			//load player object stuff
			if ((null != data.Portrait) && (null != engine.Renderer.Content))
			{
				var textureInfo = engine.Renderer.LoadImage(data.Portrait);
				Portrait = textureInfo.Texture;
			}

			base.ParseXmlData(model, engine, content);
		}

		#endregion //File IO
	}
}