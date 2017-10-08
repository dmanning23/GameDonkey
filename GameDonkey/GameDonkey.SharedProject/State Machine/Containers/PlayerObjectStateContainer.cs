using GameTimer;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameDonkeyLib
{
	/// <summary>
	/// This is a container with multiple hierarchical state machines
	/// </summary>
	public class PlayerObjectStateContainer : IStateContainer
	{
		#region Properties

		///<summary>
		///list of state machines
		///player characters use a different state machine depending on which direction they are going
		///</summary>
		public List<IStateContainer> StateContainers { get; private set; }

		/// <summary>
		/// The index of the state machine currently being used
		/// 0 = ground
		/// 1 = up
		/// 2 = down
		/// 3 = forward
		/// </summary>
		private int _currentStateMachine;

		/// <summary>
		/// The index of the state machine currently being used
		/// </summary>
		public int CurrentStateMachine
		{
			get
			{
				return _currentStateMachine;
			}
			set
			{
				if (IsCurrentStateMachineValid)
				{
					StateContainers[_currentStateMachine].StateMachine.StateChangedEvent -= StateChange;
					_currentStateMachine = value;
					StateContainers[_currentStateMachine].StateMachine.StateChangedEvent += StateChange;
				}
			}
		}

		private bool IsCurrentStateMachineValid
		{
			get
			{
				return (_currentStateMachine < StateContainers.Count);
			}
		}

		/// <summary>
		/// timer so it doesn't jump back and forth between state machines really quick
		/// </summary>
		private CountdownTimer StateMachineChangeTimer { get; set; }

		public event EventHandler<StateChangeEventArgs> StateChangedEvent;

		/// <summary>
		/// Get the number of containers, if this is a collection
		/// </summary>
		public int NumContainers
		{
			get
			{
				return StateContainers.Count;
			}
		}

		/// <summary>
		/// Get the name of this state container
		/// </summary>
		public string Name
		{
			get
			{
				//player object state containers arent named.
				return "PlayerObjectStateContainer";
			}
		}

		/// <summary>
		/// Get the current state machine for this container
		/// </summary>
		public StateMachine StateMachine
		{
			get
			{
				return StateContainers[CurrentStateMachine].StateMachine;
			}
		}

		/// <summary>
		/// Flag for whether or not we want to change bewteen state machines.
		/// Used in the state machine tool to work on one container at a time.
		/// </summary>
		public bool IgnoreStateMachineChange { get; set; }

		public int CurrentState
		{
			get
			{
				return StateContainers[CurrentStateMachine].CurrentState;
			}
		}

		public int PrevState
		{
			get
			{
				return StateContainers[CurrentStateMachine].PrevState;
			}
		}

		public int NumStates
		{
			get
			{
				return StateContainers[CurrentStateMachine].NumStates;
			}
		}

		public int NumMessages
		{
			get
			{
				//find the state machine with the most states
				int iNumMessages = StateContainers[0].NumMessages;
				for (int i = 1; i < StateContainers.Count; i++)
				{
					if (StateContainers[i].NumMessages > iNumMessages)
					{
						iNumMessages = StateContainers[i].NumMessages;
					}
				}

				return iNumMessages;
			}
		}

		public GameClock StateClock
		{
			get
			{
				return StateContainers[CurrentStateMachine].StateClock;
			}
		}

		public string CurrentStateText
		{
			get
			{
				return StateContainers[CurrentStateMachine].CurrentStateText;
			}
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// standard constructor
		/// </summary>
		public PlayerObjectStateContainer()
		{
			StateContainers = new List<IStateContainer>();
			StateMachineChangeTimer = new CountdownTimer();
			_currentStateMachine = 0;
			IgnoreStateMachineChange = false;
		}

		public virtual void LoadContent(BaseObjectModel baseObjectmodel, BaseObject owner, IGameDonkey engine, int messageOffset, ContentManager content)
		{
			throw new NotImplementedException("This method should be implemented in the child class.");
		}

		/// <summary>
		/// Reset the state machine to 
		/// </summary>
		public void Reset()
		{
			for (int i = 0; i < StateContainers.Count; i++)
			{
				StateContainers[i].Reset();
			}

			//Dont reset the state container if in toool mode
			if (!IgnoreStateMachineChange)
			{
				CurrentStateMachine = 0;
			}

			StateMachineChangeTimer.Stop();
		}

		/// <summary>
		/// method to send a message
		/// </summary>
		/// <param name="message">message to send to the state machine, 
		/// should be offset by the message offset of this dude</param>
		public void SendStateMessage(int message)
		{
			//grab onto the current state
			int iCurrentState = CurrentState;

			//check if the state change causes a state machine switch
			StateContainers[CurrentStateMachine].SendStateMessage(message);
		}

		public void ForceStateChange(int state)
		{
			//check if the state change causes a state machine switch
			StateContainers[CurrentStateMachine].ForceStateChange(state);
		}

		/// <summary>
		/// The states have changed, go through and set all the actions of the new state to "not run"
		/// if the state change was not safe, we need to pop back into the previous state.
		/// </summary>
		/// <param name="iCurState">the new state of the object</param>
		public virtual void StateChange(object sender, StateChangeEventArgs eventArgs)
		{
			//reset the current single state container
			StateContainers[CurrentStateMachine].StateChange(sender, eventArgs);

			//for all other events, fire off the event if anyone is listening
			if (null != StateChangedEvent)
			{
				StateChangedEvent(this, eventArgs);
			}
		}

		/// <summary>
		/// change the index of the state machine to use
		/// </summary>
		/// <param name="index"></param>
		public virtual void StateMachineIndex(int index, StateChangeEventArgs eventArgs)
		{
			//do a little timer so it doesn't pop back and forth between state machines...
			//but ignore it for switching into/out of ground state machine
			if (!IgnoreStateMachineChange && //Is the flag set to ignore state container changes?
				((0 == index) ||
				(0 == CurrentStateMachine) ||
				(StateMachineChangeTimer.RemainingTime <= 0.0f)))
			{
				//reset the new state machine to the initial state
				StateContainers[CurrentStateMachine].Reset();

				//switch to new state machine, which will sign up for events etc
				CurrentStateMachine = index;

				//reset the state timer
				StateMachineChangeTimer.Start(0.5f);

				//reset the single state container
				StateContainers[CurrentStateMachine].StateChange(index, eventArgs);

				//fire off the state changed event if anyone is listening
				if (null != StateChangedEvent)
				{
					StateChangedEvent(this, eventArgs);
				}
			}
			else
			{
				//NOT A SAFE STATE CHANGE!

				//unsign up for state change events of the old state machine
				StateContainers[CurrentStateMachine].StateMachine.StateChangedEvent -= this.StateChange;

				//force the state of the old state machine
				StateContainers[CurrentStateMachine].ForceStateChange(eventArgs.OldState);

				//re-sign up for state change events of the old state machine
				StateContainers[CurrentStateMachine].StateMachine.StateChangedEvent += this.StateChange;
			}
		}

		/// <summary>
		/// Execute the actions for the current state
		/// </summary>
		/// <param name="fPrevTime">the last time that the object executed actions</param>
		/// <param name="fCurTime">the time in seconds that the object has been in this state</param>
		public virtual void ExecuteActions(GameClock gameClock)
		{
			StateMachineChangeTimer.Update(gameClock);
			StateContainers[CurrentStateMachine].ExecuteActions(gameClock);
		}

		/// <summary>
		/// Check whether or not the current state is an attack state
		/// </summary>
		/// <returns></returns>
		public bool IsCurrentStateAttack()
		{
			return StateContainers[CurrentStateMachine].IsCurrentStateAttack();
		}

		/// <summary>
		/// Check if a state is an attack state
		/// </summary>
		/// <param name="state">The state to check if is an attack state</param>
		/// <returns>bool: whether or not the requested state has an attack</returns>
		public bool IsStateAttack(int state)
		{
			return StateContainers[CurrentStateMachine].IsStateAttack(state);
		}

		/// <summary>
		/// Check if an attack is active in this state.  Used to queue moves during a combo.
		/// </summary>
		/// <returns>bool: this state hasnt yet entered the recovery phase.  
		/// The move should be queued until it finishes.</returns>
		public bool IsAttackActive()
		{
			return StateContainers[CurrentStateMachine].IsAttackActive();
		}

		/// <summary>
		/// Replace all the base object pointers in this dude to point to a replacement object
		/// </summary>
		/// <param name="bot">the replacement dude</param>
		public void ReplaceOwner(BaseObject bot)
		{
			//replace in all the state actions
			for (int i = 0; i < StateContainers.Count; i++)
			{
				StateContainers[i].ReplaceOwner(bot);
			}
		}

		public int GetStateIndexFromText(string stateName)
		{
			return StateContainers[CurrentStateMachine].GetStateIndexFromText(stateName);
		}

		public int GetMessageIndexFromText(string messageName)
		{
			return StateContainers[CurrentStateMachine].GetMessageIndexFromText(messageName);
		}

		public string GetStateName(int state)
		{
			return StateContainers[CurrentStateMachine].GetStateName(state);
		}

		public string GetMessageName(int messge)
		{
			return StateContainers[CurrentStateMachine].GetMessageName(messge);
		}

		public StateActions GetStateActions(int state)
		{
			return StateContainers[CurrentStateMachine].GetStateActions(state);
		}

		public override string ToString()
		{
			return Name;
		}

		#endregion //Methods
	}
}