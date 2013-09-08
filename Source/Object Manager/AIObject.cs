using System;
using HadoukInput;
using GameTimer;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace GameDonkey
{
	public class AIObject : PlayerObject
	{
		#region Members

		/// <summary>
		/// Used to update the AI on a schedule instead of every frame.
		/// by making time length longer, AI will be easier, short time makes the AI harder
		/// </summary>
		private CountdownTimer m_UpdateTimer;

		//hard coded messages
		private int m_iJumpMessage;
		private int m_iHighJumpMessage;

		////whether or not the AI should move towards the target
		//private bool m_bMoveTowards;

		//how often the update loop should be run on this dude
		private float m_fUpdateDelta;

		static private Random g_Random = new Random(DateTime.Now.Millisecond);

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
					case 9: { m_fUpdateDelta = 0.1f; } break;
					case 8: { m_fUpdateDelta = 0.2f; } break;
					case 7: { m_fUpdateDelta = 0.3f; } break;
					case 6: { m_fUpdateDelta = 0.4f; } break;
					case 5: { m_fUpdateDelta = 0.55f; } break;
					case 4: { m_fUpdateDelta = 0.7f; } break;
					case 3: { m_fUpdateDelta = 0.85f; } break;
					case 2: { m_fUpdateDelta = 1.0f; } break;
					case 1: { m_fUpdateDelta = 1.5f; } break;
					default: { m_fUpdateDelta = -1.0f; } break;
				}
			}
		}

		#endregion

		#region Methods

		public AIObject(HitPauseClock rClock, int iQueueID) : base(EObjectType.AI, rClock, iQueueID)
		{
			m_UpdateTimer = new CountdownTimer();

			m_iJumpMessage = -1;
			m_iHighJumpMessage = -1;

			//m_bMoveTowards = false;

			m_fUpdateDelta = 1.0f;
		}

		/// <summary>
		/// Constructor for replacing a network player when they leave the game
		/// </summary>
		/// <param name="rHuman">the dude to be replaced, copy all his shit</param>
		public AIObject(PlayerObject rHuman) : base(EObjectType.AI, rHuman)
		{
			m_UpdateTimer = new CountdownTimer();

			//get those messages
			m_iJumpMessage = -1;
			m_iHighJumpMessage = -1;
			//m_iJumpMessage = States.StateMachine.GetMessageIndexFromText("Jump");
			//m_iHighJumpMessage = States.StateMachine.GetMessageIndexFromText("High Jump");
			Debug.Assert(-1 != m_iJumpMessage);
			Debug.Assert(-1 != m_iHighJumpMessage);

			//m_bMoveTowards = false;

			//this ai should be really hard since it is replacing a human
			m_fUpdateDelta = 0.1f;
		}

		public override void Update(bool bUpdateGravity)
		{
			m_UpdateTimer.Update(CharacterClock);
			base.Update(bUpdateGravity);
		}

		/// <summary>
		/// Do all the specific processing to get player input.
		/// For human players, this means getting info from the controller.
		/// For AI players, this means reacting to info in the list of "bad guys"
		/// </summary>
		/// <param name="rController">the controller for this player (bullshit and ignored for AI)</param>
		/// <param name="listBadGuys">list of all the players (ignored for human players)</param>
		public override void GetPlayerInput(InputWrapper rController, List<CPlayerQueue> listBadGuys)
		{
			Debug.Assert(null != listBadGuys);

			//check if we should update the AI
			if ((0.0f >= m_UpdateTimer.RemainingTime()) && (0.0f <= m_fUpdateDelta))
			{
				//restart the timer and run the AI update loop
				m_UpdateTimer.Start(m_fUpdateDelta);

				//loop through the "bad guys" and select a target
				BaseObject rBadGuy = null;
				Vector2 BadGuyDistance = Vector2.Zero;
				for (int i = 0; i < listBadGuys.Count; i++)
				{
					//first make sure this isn't me!
					if (GlobalID == listBadGuys[i].Character.GlobalID)
					{
						continue;
					}

					//go through ALL the active objects in the player queue so AI will react correctly to projectiles
					for (int j = 0; j < listBadGuys[i].ActiveObjects.Count; j++)
					{
						//get the distance to this dude
						Vector2 distance = listBadGuys[i].ActiveObjects[j].Position - Position;
						if ((null == rBadGuy) || (BadGuyDistance.LengthSquared() > distance.LengthSquared()))
						{
							rBadGuy = listBadGuys[i].ActiveObjects[j];
							BadGuyDistance = distance;
						}
					}
				}

				if (null == rBadGuy)
				{
					//if AI wins a stock match, there won't be any bad guys
					return;
				}

				//react to the target

				//do i need to turn around?
				if (BadGuyDistance.X <= 0.0f)
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
				if ((BadGuyDistance.X > HalfHeight) || (BadGuyDistance.X < (-1.0 * HalfHeight)))
				{
					//the bad guy is to the left or right, move towards the target
					//m_bMoveTowards = true;
				}

				//the target is far away, but is it above me?
				if (BadGuyDistance.Y < (-2.0f * HalfHeight))
				{
					////teh bad guy is waaay above me, super jump at them
					//Debug.Assert(-1 != m_iHighJumpMessage);
					//SendAttackMessage(m_iHighJumpMessage);
				}
				else if (BadGuyDistance.Y < (-1.0f * HalfHeight))
				{
					////jump at the target
					//Debug.Assert(-1 != m_iJumpMessage);
					//SendAttackMessage(m_iJumpMessage);
				}

				//how far away is the target?
				if (BadGuyDistance.LengthSquared() <= (Height * Height))
				{
					//the target must be close!

					//is the target attacking?
					if (rBadGuy.States.IsCurrentStateAttack())
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
		public override void CheckHardCodedStates(InputWrapper rController)
		{
			int iCurrentState = States.CurrentState();

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

			base.CheckHardCodedStates(rController);
		}

		/// <summary>
		/// convert the ai difficulty to an integer
		/// </summary>
		/// <returns>int: number bewteen 1 and 10 signifying the difficulty of the AI</returns>
		public int ConvertAIToInt()
		{
			if ((m_fUpdateDelta >= 0.0f) && (m_fUpdateDelta < 0.2f))
			{
				return 9;
			}
			else if ((m_fUpdateDelta >= 0.2f) && (m_fUpdateDelta < 0.3f))
			{
				return 8;
			}
			else if ((m_fUpdateDelta >= 0.3f) && (m_fUpdateDelta < 0.4f))
			{
				return 7;
			}
			else if ((m_fUpdateDelta >= 0.4f) && (m_fUpdateDelta < 0.55f))
			{
				return 6;
			}
			else if ((m_fUpdateDelta >= 0.55f) && (m_fUpdateDelta < 0.7f))
			{
				return 5;
			}
			else if ((m_fUpdateDelta >= 0.7f) && (m_fUpdateDelta < 0.85f))
			{
				return 4;
			}
			else if ((m_fUpdateDelta >= 0.85f) && (m_fUpdateDelta < 1.0f))
			{
				return 3;
			}
			else if ((m_fUpdateDelta >= 1.0f) && (m_fUpdateDelta < 1.5f))
			{
				return 2;
			}
			else if ((m_fUpdateDelta >= 1.5f) && (m_fUpdateDelta < 2.0))
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
		/// <param name="bIncrease">whether to increase or decrease the AI difficulty</param>
		public void ChangeAIDifficulty(bool bIncrease)
		{
			if (m_fUpdateDelta <= 0.1f)
			{
				if (bIncrease)
				{
				}
				else
				{
					m_fUpdateDelta = 0.2f;
				}
			}
			else if ((m_fUpdateDelta >= 0.2f) && (m_fUpdateDelta < 0.3f))
			{
				if (bIncrease)
				{
					m_fUpdateDelta = 0.1f;
				}
				else
				{
					m_fUpdateDelta = 0.3f;
				}
			}
			else if ((m_fUpdateDelta >= 0.3f) && (m_fUpdateDelta < 0.4f))
			{
				if (bIncrease)
				{
					m_fUpdateDelta = 0.2f;
				}
				else
				{
					m_fUpdateDelta = 0.4f;
				}
			}
			else if ((m_fUpdateDelta >= 0.4f) && (m_fUpdateDelta < 0.55f))
			{
				if (bIncrease)
				{
					m_fUpdateDelta = 0.3f;
				}
				else
				{
					m_fUpdateDelta = 0.55f;
				}
			}
			else if ((m_fUpdateDelta >= 0.55f) && (m_fUpdateDelta < 0.7f))
			{
				if (bIncrease)
				{
					m_fUpdateDelta = 0.4f;
				}
				else
				{
					m_fUpdateDelta = 0.7f;
				}
			}
			else if ((m_fUpdateDelta >= 0.7f) && (m_fUpdateDelta < 0.85f))
			{
				if (bIncrease)
				{
					m_fUpdateDelta = 0.55f;
				}
				else
				{
					m_fUpdateDelta = 0.85f;
				}
			}
			else if ((m_fUpdateDelta >= 0.85f) && (m_fUpdateDelta < 1.0f))
			{
				if (bIncrease)
				{
					m_fUpdateDelta = 0.7f;
				}
				else
				{
					m_fUpdateDelta = 1.0f;
				}
			}
			else if ((m_fUpdateDelta >= 1.0f) && (m_fUpdateDelta < 1.5f))
			{
				if (bIncrease)
				{
					m_fUpdateDelta = 0.85f;
				}
				else
				{
					m_fUpdateDelta = 1.5f;
				}
			}
			else if ((m_fUpdateDelta >= 1.5f) && (m_fUpdateDelta < 2.0f))
			{
				if (bIncrease)
				{
					m_fUpdateDelta = 1.0f;
				}
				else
				{
					m_fUpdateDelta = 2.0f;
				}
			}
			else
			{
				if (bIncrease)
				{
					m_fUpdateDelta = 1.5f;
				}
				else
				{
				}
			}
		}

		#endregion //Methods
	}
}