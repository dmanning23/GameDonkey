using GameTimer;
using HadoukInput;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace GameDonkeyLib
{
	public class AIObject : PlayerObject
	{
		#region Members

		/// <summary>
		/// Used to update the AI on a schedule instead of every frame.
		/// by making time length longer, AI will be easier, short time makes the AI harder
		/// </summary>
		private CountdownTimer UpdateTimer { get; set; }

		//hard coded messages
		private int JumpMessage { get; set; }
		private int HighJumpMessage { get; set; }

		////whether or not the AI should move towards the target
		//private bool m_bMoveTowards;

		//how often the update loop should be run on this dude
		private float UpdateDelta { get; set; }

		static private Random _random = new Random(DateTime.Now.Millisecond);

		#endregion //Members

		#region properties

		protected float HalfHeight
		{
			get { return Height * 0.5f; }
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

		#endregion

		#region Methods

		public AIObject(HitPauseClock clock, int queueId) : base(GameObjectType.AI, clock, queueId)
		{
			UpdateTimer = new CountdownTimer();

			JumpMessage = -1;
			HighJumpMessage = -1;

			//m_bMoveTowards = false;

			UpdateDelta = 1.0f;
		}

		public override void Update()
		{
			UpdateTimer.Update(CharacterClock);
			base.Update();
		}

		/// <summary>
		/// Do all the specific processing to get player input.
		/// For human players, this means getting info from the controller.
		/// For AI players, this means reacting to info in the list of "bad guys"
		/// </summary>
		/// <param name="controller">the controller for this player (bullshit and ignored for AI)</param>
		/// <param name="listBadGuys">list of all the players (ignored for human players)</param>
		public override void GetPlayerInput(InputWrapper controller, List<PlayerQueue> listBadGuys)
		{
			//check if we should update the AI
			if ((0.0f >= UpdateTimer.RemainingTime) && (0.0f <= UpdateDelta))
			{
				//restart the timer and run the AI update loop
				UpdateTimer.Start(UpdateDelta);

				//loop through the "bad guys" and select a target
				BaseObject badGuy = null;
				var badGuyDistance = Vector2.Zero;
				for (var i = 0; i < listBadGuys.Count; i++)
				{
					//first make sure this isn't me!
					if (Id == listBadGuys[i].Character.Id)
					{
						continue;
					}

					//go through ALL the active objects in the player queue so AI will react correctly to projectiles
					for (var j = 0; j < listBadGuys[i].Active.Count; j++)
					{
						//get the distance to this dude
						var distance = listBadGuys[i].Active[j].Position - Position;
						if ((null == badGuy) || (badGuyDistance.LengthSquared() > distance.LengthSquared()))
						{
							badGuy = listBadGuys[i].Active[j];
							badGuyDistance = distance;
						}
					}
				}

				if (null == badGuy)
				{
					//if AI wins a stock match, there won't be any bad guys
					return;
				}

				//react to the target

				//do i need to turn around?
				if (badGuyDistance.X <= 0.0f)
				{
					//the bad guy is to the left of me
					if (!Flip)
					{
						//TODO: start moving the AI in that direction?
						//SendTurnAroundMessage();
					}
				}
				else
				{
					//the BadGuyDistance guy is to the right
					if (Flip)
					{
						//TODO: start moving the AI in that direction?
						//SendTurnAroundMessage();
					}
				}

				//reset the "move towards" flag
				//m_bMoveTowards = false;

				//shoudl i move towards the target?
				if ((badGuyDistance.X > HalfHeight) || (badGuyDistance.X < (-1.0 * HalfHeight)))
				{
					//the bad guy is to the left or right, move towards the target
					//m_bMoveTowards = true;
				}

				//the target is far away, but is it above me?
				if (badGuyDistance.Y < (-2.0f * HalfHeight))
				{
					////teh bad guy is waaay above me, super jump at them
					//Debug.Assert(-1 != m_iHighJumpMessage);
					//SendAttackMessage(m_iHighJumpMessage);
				}
				else if (badGuyDistance.Y < (-1.0f * HalfHeight))
				{
					////jump at the target
					//Debug.Assert(-1 != m_iJumpMessage);
					//SendAttackMessage(m_iJumpMessage);
				}

				//how far away is the target?
				if (badGuyDistance.LengthSquared() <= (Height * Height))
				{
					//the target must be close!

					//is the target attacking?
					if (badGuy.States.IsCurrentStateAttack())
					{
						//select a defensive option
						SelectDefensiveOption();
					}
					else
					{
						//try to attack the target

						//select an offensive option
						SelectOffensiveOption();
					}
				}
			}
		}

		private void SelectDefensiveOption()
		{
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
		}

		private void SelectOffensiveOption()
		{
			////select a random attack and execute it
			//int iMin = (TurnAroundMessage - m_States.StateMachine.MessageOffset) + 1;
			//int iMax = m_States.StateMachine.NumMessages - iMin;

			//int iAttack = ((g_Random.Next() % iMax) + iMin);
			//Debug.Assert(iAttack >= 0);
			//Debug.Assert(iAttack > (TurnAroundMessage - m_States.StateMachine.MessageOffset));
			//Debug.Assert(iAttack < m_States.StateMachine.NumMessages);

			//SendAttackMessage(iAttack + m_States.StateMachine.MessageOffset);

			//SendAttackMessage((int)EState.Quick);
		}

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
		public override void CheckHardCodedStates()
		{
			int currentState = States.CurrentState;

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

			base.CheckHardCodedStates();
		}

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