using System.Diagnostics;
using GameTimer;
using Microsoft.Xna.Framework.Content;
#if !NO_NETWORKING
using Microsoft.Xna.Framework.Net;
#endif
using StateMachineBuddy;
using System;

namespace GameDonkey
{
	/// <summary>
	/// This a state container with one state machine
	/// Heads up, this state machine doesn't sign up for all the state change messages...
	/// It's meant to be used with playerstatecontainer, otherwise use the singlestatecontainer
	/// </summary>
	class CSingleStateContainer : IStateContainer
	{
		#region Members

		public event EventHandler<StateChangeEventArgs> StateChangedContainerEvent;

		public StateMachine StateMachine { get; private set; }

		private CStateActionsList m_listActions;

		/// <summary>
		/// This clock times how long the character has been in the current state
		/// </summary>
		protected GameClock m_StateClock;

		public event EventHandler<StateChangeEventArgs> StateChangedEvent;

		#endregion //Members

		#region Methods

		/// <summary>
		/// contructor
		/// </summary>
		/// <param name="myStateMachine">the state machine this dude will use</param>
		public CSingleStateContainer(StateMachine myStateMachine)
		{
			StateMachine = myStateMachine;
			m_listActions = new CStateActionsList();
			m_StateClock = new GameClock();

			//This container only signs up for the reset event
			StateMachine.ResetEvent += this.StateChange;
		}

		public void Reset()
		{
			Debug.Assert(null != StateMachine);
			StateMachine.ResetToInitialState();
		}

		/// <summary>
		/// method to send a message
		/// </summary>
		/// <param name="iMessage">message to send to the state machine, 
		/// should be offset by the message offset of this dude</param>
		/// <returns>bool: did it change states?</returns>
		public void SendStateMessage(int iMessage)
		{
			StateMachine.SendStateMessage(iMessage);
		}

		public void ForceStateChange(int iState)
		{
			StateMachine.ForceState(iState);
		}

		/// <summary>
		/// The states have changed, go through and set all the actions of the new state to "not run"
		/// </summary>
		/// <param name="iCurState">the new state of the object</param>
		public void StateChange(object sender, StateChangeEventArgs eventArgs)
		{
			//set the new state actions to 'not run'
			m_listActions.StateChange(eventArgs.NewState);

			//restart the state clock
			m_StateClock.Start();
			m_StateClock.TimeDelta = 0.0f;

			//fire off the event if anyone is listening
			if (null != StateChangedContainerEvent)
			{
				StateChangedContainerEvent(this, eventArgs);
			}
		}

		/// <summary>
		/// Execute the actions for the current state
		/// </summary>
		/// <param name="fPrevTime">the last time that the object executed actions</param>
		/// <param name="fCurTime">the time in seconds that the object has been in this state</param>
		public void ExecuteActions(GameClock rGameClock)
		{
			m_StateClock.Update(rGameClock);

			//execute the correct action container
			Debug.Assert(StateMachine.CurrentState >= 0);
			m_listActions.ExecuteActions(m_StateClock, StateMachine.CurrentState);
		}

		/// <summary>
		/// Check whether or not the current state is an attack state
		/// </summary>
		/// <returns></returns>
		public bool IsCurrentStateAttack()
		{
			Debug.Assert(StateMachine.CurrentState >= 0);
			return IsStateAttack(StateMachine.CurrentState);
		}

		/// <summary>
		/// Check if a state is an attack state
		/// </summary>
		/// <param name="iState">The state to check if is an attack state</param>
		/// <returns>bool: whether or not the requested state has an attack</returns>
		public bool IsStateAttack(int iState)
		{
			Debug.Assert(iState >= 0);
			return m_listActions.IsStateAttack(iState);
		}

		/// <summary>
		/// Check if an attack is active in this state.  Used to queue moves during a combo.
		/// </summary>
		/// <returns>bool: this state hasnt yet entered the recovery phase.  
		/// The move should be queued until it finishes.</returns>
		public bool IsAttackActive()
		{
			Debug.Assert(StateMachine.CurrentState >= 0);

			//check if the current state is an attack state, and if an attack is active
			return m_listActions.IsAttackActive(m_StateClock, StateMachine.CurrentState);
		}

		/// <summary>
		/// Replace all the base object pointers in this dude to point to a replacement object
		/// </summary>
		/// <param name="myBot">the replacement dude</param>
		public void ReplaceOwner(BaseObject myBot)
		{
			//replace in all the state actions
			m_listActions.ReplaceOwner(myBot);
		}

		public int CurrentState()
		{
			Debug.Assert(null != StateMachine);
			return StateMachine.CurrentState;
		}

		public int PrevState()
		{
			Debug.Assert(null != StateMachine);
			return StateMachine.PrevState;
		}

		public int GetStateIndexFromText(string strStateName)
		{
			Debug.Assert(null != StateMachine);
			return StateMachine.GetStateIndexFromText(strStateName);
		}

		public int GetMessageIndexFromText(string strMessageName)
		{
			Debug.Assert(null != StateMachine);
			return StateMachine.GetMessageIndexFromText(strMessageName);
		}

		public string GetStateName(int iStateIndex)
		{
			Debug.Assert(null != StateMachine);
			return StateMachine.GetStateName(iStateIndex);
		}

		public string GetMessageName(int iMessageIndex)
		{
			Debug.Assert(null != StateMachine);
			return StateMachine.GetMessageName(iMessageIndex);
		}

		public int NumStates()
		{
			Debug.Assert(null != StateMachine);
			return StateMachine.NumStates;
		}

		public int NumMessages()
		{
			Debug.Assert(null != StateMachine);
			return StateMachine.NumMessages;
		}

		public CStateActions GetStateActions(int iStateIndex)
		{
			Debug.Assert(0 <= iStateIndex);
			Debug.Assert(iStateIndex < NumStates());
			return m_listActions.GetStateActions(iStateIndex);
		}

		public GameClock GetStateClock()
		{
			return m_StateClock;
		}

		public string CurrentStateText()
		{
			return StateMachine.CurrentStateText;
		}

		#endregion //Methods

		#region Networking

#if !NO_NETWORKING

		/// <summary>
		/// Read this object from a network packet reader.
		/// </summary>
		public void ReadFromNetwork(PacketReader packetReader)
		{
			StateMachine.ReadFromNetwork(packetReader);
			m_StateClock.ReadFromNetwork(packetReader);
		}

		/// <summary>
		/// Write this object to a network packet reader.
		/// </summary>
		public void WriteToNetwork(PacketWriter packetWriter)
		{
			StateMachine.WriteToNetwork(packetWriter);
			m_StateClock.WriteToNetwork(packetWriter);
		}

#endif

		#endregion //Networking

		#region State Action File IO

#if WINDOWS

		public bool ReadSerializedStateActions(string strFilename, BaseObject rOwner, IGameDonkey rEngine)
		{
			return m_listActions.ReadSerializedStateActions(strFilename, rOwner, rEngine, StateMachine);
		}

		public bool WriteStateActions(string strFilename)
		{
			return m_listActions.WriteStateActions(strFilename);
		}

#endif

		public void ReadSerializedStateActions(ContentManager rXmlContent,
			string strResource,
			BaseObject rOwner,
			IGameDonkey rEngine)
		{
			Debug.Assert(null != StateMachine);
			m_listActions.ReadSerializedStateActions(rXmlContent, strResource, rOwner, StateMachine, rEngine);
		}

		#endregion //State Action File IO

		#region State Machine File IO

#if WINDOWS

		public bool ReadStateMachineFile(string strFilename)
		{
			return StateMachine.ReadSerializedFile(strFilename);
		}

		public bool AppendStateMachineFile(string strFilename)
		{
			Debug.Assert(null != StateMachine);
			return StateMachine.AppendSerializedFile(strFilename);
		}

#endif

		public bool ReadStateMachineFile(ContentManager rContent, string strResource, int iMessageOffset)
		{
			Debug.Assert(null != StateMachine);
			return StateMachine.ReadSerializedFile(rContent, strResource, iMessageOffset);
		}

		public bool AppendStateMachineFile(ContentManager rContent, string strResource, int iMessageOffset)
		{
			Debug.Assert(null != StateMachine);
			return StateMachine.AppendSerializedFile(rContent, strResource, iMessageOffset);
		}

		#endregion //File IO

		#region Combined File IO

#if WINDOWS

		public bool ReadStateContainer(string strStateMachineFilename,
			int iMessageOffset,
			string strStateActionsFilename,
			BaseObject rOwner,
			IGameDonkey rEngine,
			bool bPlayerStateMachine,
			bool bFlyingStateMachine)
		{
			//create the correct state machine
			Debug.Assert(null != StateMachine);
			if (bPlayerStateMachine)
			{
				Debug.Assert(StateMachine is CWeddingStateMachine);

				//load the state machine
				if (!StateMachine.AppendSerializedFile(strStateMachineFilename))
				{
					return false;
				}
			}
			else
			{
				//load the state machine
				if (!StateMachine.ReadSerializedFile(strStateMachineFilename))
				{
					return false;
				}
			}

			//load the state actions
			Debug.Assert(null != m_listActions);
			if (!m_listActions.ReadSerializedStateActions(strStateActionsFilename, rOwner, rEngine, StateMachine))
			{
				return false;
			}

			return true;
		}

#endif

		public void ReadStateContainer(ContentManager rContent,
			string strStateMachineResource,
			int iMessageOffset,
			string strStateActionsResource,
			BaseObject rOwner,
			IGameDonkey rEngine,
			bool bPlayerStateMachine,
			bool bFlyingStateMachine)
		{
			//create the correct state machine
			Debug.Assert(null != StateMachine);
			if (bPlayerStateMachine)
			{
				Debug.Assert(StateMachine is CWeddingStateMachine);

				//load the state machine
				StateMachine.AppendSerializedFile(rContent, strStateMachineResource, iMessageOffset);
			}
			else
			{
				StateMachine = new StateMachine();

				//load the state machine
				StateMachine.ReadSerializedFile(rContent, strStateMachineResource, iMessageOffset);
			}

			//load the state actions
			Debug.Assert(null != m_listActions);
			m_listActions.ReadSerializedStateActions(rContent, strStateActionsResource, rOwner, StateMachine, rEngine);
		}

		#endregion //Combined File IO
	}
}