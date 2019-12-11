using GameTimer;
using HadoukInput;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace GameDonkeyLib
{
	/// <summary>
	/// This is a controller thing that is used to control a PlayerObject
	/// </summary>
	public abstract class AIController
	{
		#region Properties

		protected BaseObject Player { get; set; }

		/// <summary>
		/// Used to update the AI on a schedule instead of every frame.
		/// by making time length longer, AI will be easier, short time makes the AI harder
		/// </summary>
		private CountdownTimer UpdateTimer { get; set; }

		/// <summary>
		/// how often the update loop should be run on this dude
		/// </summary>
		private float _updateDelta;
		private float UpdateDelta
		{
			get
			{
				return _updateDelta;
			}
			set
			{
				_updateDelta = value;
				UpdateAimDelta = UpdateDelta * 0.3f;
			}
		}

		/// <summary>
		/// Used to update the AI on a schedule instead of every frame.
		/// by making time length longer, AI will be easier, short time makes the AI harder
		/// </summary>
		private CountdownTimer AimTimer { get; set; }

		/// <summary>
		/// how often the update loop should be run on this dude
		/// </summary>
		private float UpdateAimDelta { get; set; }

		static private Random _random = new Random(DateTime.Now.Millisecond);

		protected float HalfHeight
		{
			get { return Player.Height * 0.5f; }
		}

		public int Difficulty
		{
			get { return ConvertAIToInt(); }
			set
			{
				switch (value)
				{
					case 9: { UpdateDelta = 0.1f; } break;
					case 8: { UpdateDelta = 0.2f; } break;
					case 7: { UpdateDelta = 0.3f; } break;
					case 6: { UpdateDelta = 0.4f; } break;
					case 5: { UpdateDelta = 0.55f; } break;
					case 4: { UpdateDelta = 0.7f; } break;
					case 3: { UpdateDelta = 0.85f; } break;
					case 2: { UpdateDelta = 1.0f; } break;
					case 1: { UpdateDelta = 1.5f; } break;
					default: { UpdateDelta = -1.0f; } break;
				}
			}
		}

		/// <summary>
		/// The direction this AI wants to go
		/// </summary>
		private Vector2 _direction;
		public Vector2 Direction
		{
			get
			{
				return _direction;
			}
			set
			{
				_direction = value;
			}
		}

		protected abstract int AttackDistance { get; }

		protected abstract int DefendDistance { get; }

		protected BaseObject BadGuy { get; set; }

		protected Vector2 BadGuyDistance { get; set; }

		#endregion //Properties

		#region Methods

		public AIController(PlayerObject player)
		{
			Player = player;
			UpdateTimer = new CountdownTimer();
			AimTimer = new CountdownTimer();

			UpdateDelta = 1.0f;
		}

		public virtual void Update()
		{
			UpdateTimer.Update(Player.CharacterClock);
			AimTimer.Update(Player.CharacterClock);
		}

		/// <summary>
		/// Do all the specific processing to get player input.
		/// For human players, this means getting info from the controller.
		/// For AI players, this means reacting to info in the list of "bad guys"
		/// </summary>
		/// <param name="controller">the controller for this player (bullshit and ignored for AI)</param>
		/// <param name="listBadGuys">list of all the players (ignored for human players)</param>
		public void GetPlayerInput(List<PlayerQueue> listBadGuys, bool ignoreAttackInput)
		{
			//check if we should update the target
			if (!AimTimer.HasTimeRemaining && (0.0f <= UpdateAimDelta))
			{
				AimTimer.Start(UpdateAimDelta);

				//loop through the "bad guys" and select a target
				BadGuy = null;
				BadGuyDistance = Vector2.Zero;
				for (var i = 0; i < listBadGuys.Count; i++)
				{
					//first make sure this isn't me!
					if (Player.QueueId == listBadGuys[i].QueueId)
					{
						continue;
					}

					//go through ALL the active objects in the player queue so AI will react correctly to projectiles
					for (var j = 0; j < listBadGuys[i].Active.Count; j++)
					{
						//get the distance to this dude
						var distance = listBadGuys[i].Active[j].Position - Player.Position;
						if ((null == BadGuy) || (BadGuyDistance.LengthSquared() > distance.LengthSquared()))
						{
							BadGuy = listBadGuys[i].Active[j];
							BadGuyDistance = distance;
						}
					}
				}

				if (null != BadGuy && BadGuyDistance.LengthSquared() > 0f)
				{
					//set the direction
					Direction = Vector2.Normalize(BadGuyDistance);
					_direction.Y = _direction.Y * -1f;
				}
			}

			//check if we should update the AI
			if (!UpdateTimer.HasTimeRemaining && (0.0f <= UpdateDelta))
			{
				//restart the timer and run the AI update loop
				UpdateTimer.Start(UpdateDelta);

				if (null == BadGuy || BadGuyDistance.LengthSquared() == 0f)
				{
					//if AI wins a stock match, there won't be any bad guys
					return;
				}

				//react to the target

				//do i need to turn around?
				if (BadGuyDistance.X <= 0.0f)
				{
					//the bad guy is to the left of me
					if (!Player.Flip)
					{
						SendTurnAroundMessage();
					}
				}
				else
				{
					//the BadGuyDistance guy is to the right
					if (Player.Flip)
					{
						SendTurnAroundMessage();
					}
				}

				//shoudl i move towards the target?
				if ((BadGuyDistance.X > HalfHeight) || (BadGuyDistance.X < (-1.0 * HalfHeight)))
				{
					//the bad guy is to the left or right, move towards the target
					SendWalkMessage();
				}
				else
				{
					//SendDoneMessage();
				}

				//the target is far away, but is it above me?
				if (BadGuyDistance.Y < (-2.0f * HalfHeight))
				{
					//teh bad guy is waaay above me, super jump at them
					SendHighJumpMessage();
				}
				else if (BadGuyDistance.Y < (-1.0f * HalfHeight))
				{
					//jump at the target
					SendJumpMessage();
				}

				//is the target attacking?
				var blocking = false;
				if (BadGuy.States.IsCurrentStateAttack() && BadGuyDistance.LengthSquared() <= (DefendDistance * DefendDistance))
				{
					//select a defensive option
					blocking = SelectDefensiveOption();
				}

				//If we aren't trying to block an attack and the target is in distance, take a swing at them.
				if (!blocking && BadGuyDistance.LengthSquared() <= (AttackDistance * AttackDistance) && !ignoreAttackInput)
				{
					//the target must be close! try to attack the target
					SelectOffensiveOption();
				}
			}
		}

		protected abstract void SendTurnAroundMessage();

		protected abstract void SendWalkMessage();

		protected abstract void SendDoneMessage();

		protected abstract void SendHighJumpMessage();

		protected abstract void SendJumpMessage();

		protected abstract bool SelectDefensiveOption();
		
			//TODO: should i block or evade?
			//if ((g_Random.Next() % 2) == 0)
			//{
			//    //block
			//    Debug.Assert(-1 != BlockMessage);
			//    SendAttackMessage(BlockMessage);
			//}
			//else
			//{
			//    //evade
			//    Debug.Assert(-1 != DashMessage);
			//    SendAttackMessage(DashMessage);
			//}
		

		protected abstract void SelectOffensiveOption();
		
			////select a random attack and execute it
			//int iMin = (TurnAroundMessage - m_States.StateMachine.MessageOffset) + 1;
			//int iMax = m_States.StateMachine.NumMessages - iMin;

			//int iAttack = ((g_Random.Next() % iMax) + iMin);
			//Debug.Assert(iAttack >= 0);
			//Debug.Assert(iAttack > (TurnAroundMessage - m_States.StateMachine.MessageOffset));
			//Debug.Assert(iAttack < m_States.StateMachine.NumMessages);

			//SendAttackMessage(iAttack + m_States.StateMachine.MessageOffset);

			//SendAttackMessage((int)EState.Quick);
		

		///// <summary>
		///// This is used to send attack moves from the input queue to the state machine, through the combo engine
		///// </summary>
		///// <param name="iMessage"></param>
		//protected override void SendAttackMessage(int iNextMoov)
		//{
		//    if (0.0f <= m_fUpdateDelta)
		//    {
		//        base.SendAttackMessage(iNextMoov);
		//    }
		//}

		/// <summary>
		/// Check if the play is in a hard coded state,
		/// if so, check if its button is still being held down
		/// </summary>
		//public override void CheckHardCodedStates()
		//{
		//	var currentState = States.CurrentState;

			////check if forward button is held down, add forward movement to the player 
			//if (m_bMoveTowards)
			//{
			//    //only check for standing, walking, falling states
			//    if ((iCurrentState == WalkingState) ||
			//        FallingState ||
			//        (iCurrentState == JumpingState) ||
			//        (iCurrentState == HighJumpState))
			//    {
			//        Accelerate();
			//    }
			//    else
			//    {
			//        SendStateMessage(WalkMessage);
			//    }
			//}
			//else
			//{
			//    //user is no longer holding the forward direction
			//    if (iCurrentState == WalkingState)
			//    {
			//        SendStateMessage(DoneMessage);
			//    }
			//    else if (!States.IsStateAttack(iCurrentState) && 
			//        (StunnedState != iCurrentState) &&
			//        !DashState)
			//    {
			//        //move the x velocity to 0
			//        Deccelerate();
			//    }
			//}

			//if (BlockingState)
			//{
			//    //check the block button
			//    if (!m_bBlocking)
			//    {
			//        SendStateMessage(DoneMessage);
			//    }
			//}

		//	base.CheckHardCodedStates();
		//}

		/// <summary>
		/// convert the ai difficulty to an integer
		/// </summary>
		/// <returns>int: number bewteen 1 and 10 signifying the difficulty of the AI</returns>
		public int ConvertAIToInt()
		{
			if ((UpdateDelta >= 0.0f) && (UpdateDelta < 0.2f))
			{
				return 9;
			}
			else if ((UpdateDelta >= 0.2f) && (UpdateDelta < 0.3f))
			{
				return 8;
			}
			else if ((UpdateDelta >= 0.3f) && (UpdateDelta < 0.4f))
			{
				return 7;
			}
			else if ((UpdateDelta >= 0.4f) && (UpdateDelta < 0.55f))
			{
				return 6;
			}
			else if ((UpdateDelta >= 0.55f) && (UpdateDelta < 0.7f))
			{
				return 5;
			}
			else if ((UpdateDelta >= 0.7f) && (UpdateDelta < 0.85f))
			{
				return 4;
			}
			else if ((UpdateDelta >= 0.85f) && (UpdateDelta < 1.0f))
			{
				return 3;
			}
			else if ((UpdateDelta >= 1.0f) && (UpdateDelta < 1.5f))
			{
				return 2;
			}
			else if ((UpdateDelta >= 1.5f) && (UpdateDelta < 2.0))
			{
				return 1;
			}
			else
			{
				return 0;
			}
		}

		/// <summary>
		/// Change the ai difficulty
		/// </summary>
		/// <param name="increase">whether to increase or decrease the AI difficulty</param>
		public void ChangeAIDifficulty(bool increase)
		{
			if (UpdateDelta <= 0.1f)
			{
				if (increase)
				{
				}
				else
				{
					UpdateDelta = 0.2f;
				}
			}
			else if ((UpdateDelta >= 0.2f) && (UpdateDelta < 0.3f))
			{
				if (increase)
				{
					UpdateDelta = 0.1f;
				}
				else
				{
					UpdateDelta = 0.3f;
				}
			}
			else if ((UpdateDelta >= 0.3f) && (UpdateDelta < 0.4f))
			{
				if (increase)
				{
					UpdateDelta = 0.2f;
				}
				else
				{
					UpdateDelta = 0.4f;
				}
			}
			else if ((UpdateDelta >= 0.4f) && (UpdateDelta < 0.55f))
			{
				if (increase)
				{
					UpdateDelta = 0.3f;
				}
				else
				{
					UpdateDelta = 0.55f;
				}
			}
			else if ((UpdateDelta >= 0.55f) && (UpdateDelta < 0.7f))
			{
				if (increase)
				{
					UpdateDelta = 0.4f;
				}
				else
				{
					UpdateDelta = 0.7f;
				}
			}
			else if ((UpdateDelta >= 0.7f) && (UpdateDelta < 0.85f))
			{
				if (increase)
				{
					UpdateDelta = 0.55f;
				}
				else
				{
					UpdateDelta = 0.85f;
				}
			}
			else if ((UpdateDelta >= 0.85f) && (UpdateDelta < 1.0f))
			{
				if (increase)
				{
					UpdateDelta = 0.7f;
				}
				else
				{
					UpdateDelta = 1.0f;
				}
			}
			else if ((UpdateDelta >= 1.0f) && (UpdateDelta < 1.5f))
			{
				if (increase)
				{
					UpdateDelta = 0.85f;
				}
				else
				{
					UpdateDelta = 1.5f;
				}
			}
			else if ((UpdateDelta >= 1.5f) && (UpdateDelta < 2.0f))
			{
				if (increase)
				{
					UpdateDelta = 1.0f;
				}
				else
				{
					UpdateDelta = 2.0f;
				}
			}
			else
			{
				if (increase)
				{
					UpdateDelta = 1.5f;
				}
				else
				{
				}
			}
		}

		#endregion //Methods
	}
}