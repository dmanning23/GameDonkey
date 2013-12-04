using System.Collections.Generic;
using GameTimer;
using System.Diagnostics;
using Microsoft.Xna.Framework.Content;
#if NETWORKING
using Microsoft.Xna.Framework.Net;
#endif
using StateMachineBuddy;
using System;

namespace GameDonkey
{
	/// <summary>
	/// This is a container with multiple hierarchical state machines
	/// </summary>
	class PlayerObjectStateContainer : IStateContainer
	{
		#region Members

		///<summary>
		///list of state machines
		///player characters use a different state machine depending on which direction they are going
		///</summary>
		private List<IStateContainer> m_StateMachines;

		/// <summary>
		/// The index of the state machine currently being used
		/// 0 = ground
		/// 1 = up
		/// 2 = down
		/// 3 = forward
		/// </summary>
		private int _currentStateMachine;

		private int CurrentStateMachine
		{
			get
			{
				return _currentStateMachine;
			}
			set
			{
				m_StateMachines[_currentStateMachine].StateMachine.StateChangedEvent -= StateChange;
				_currentStateMachine = value;
				m_StateMachines[_currentStateMachine].StateMachine.StateChangedEvent += StateChange;

			}
		}

		/// <summary>
		/// timer so it doesn't jump back and forth between state machines really quick
		/// </summary>
		private CountdownTimer m_StateMachineChangeTimer;

		public event EventHandler<StateChangeEventArgs> StateChangedEvent;

		#endregion //Members

		#region Properties

		/// <summary>
		/// Get the number of containers, if this is a collection
		/// </summary>
		public int NumContainers
		{
			get
			{
				return m_StateMachines.Count;
			}
		}

		/// <summary>
		/// Get a list of all the containers in this dude.
		/// </summary>
		public List<IStateContainer> Containers
		{
			get
			{
				return m_StateMachines;
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
				Debug.Assert(null != m_StateMachines);
				Debug.Assert(CurrentStateMachine >= 0);
				Debug.Assert(CurrentStateMachine < m_StateMachines.Count);

				return m_StateMachines[CurrentStateMachine].StateMachine;
			}
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// standard constructor
		/// </summary>
		public PlayerObjectStateContainer()
		{
			m_StateMachines = new List<IStateContainer>();
			m_StateMachineChangeTimer = new CountdownTimer();
			_currentStateMachine = 0;
		}

		/// <summary>
		/// Reset the state machine to 
		/// </summary>
		public void Reset()
		{
			Debug.Assert(null != m_StateMachines);

			for (int i = 0; i < m_StateMachines.Count; i++)
			{
				Debug.Assert(null != m_StateMachines[i]);
				m_StateMachines[i].Reset();
			}
			CurrentStateMachine = 0;

			Debug.Assert(null != m_StateMachineChangeTimer);
			m_StateMachineChangeTimer.Stop();
		}

		/// <summary>
		/// method to send a message
		/// </summary>
		/// <param name="iMessage">message to send to the state machine, 
		/// should be offset by the message offset of this dude</param>
		public void SendStateMessage(int iMessage)
		{
			Debug.Assert(null != m_StateMachines);
			Debug.Assert(_currentStateMachine >= 0);
			Debug.Assert(_currentStateMachine < m_StateMachines.Count);

			//grab onto the current state
			int iCurrentState = CurrentState();

			//check if the state change causes a state machine switch
			m_StateMachines[CurrentStateMachine].SendStateMessage(iMessage);
		}

		public void ForceStateChange(int iState)
		{
			Debug.Assert(null != m_StateMachines);
			Debug.Assert(CurrentStateMachine >= 0);
			Debug.Assert(CurrentStateMachine < m_StateMachines.Count);

			//check if the state change causes a state machine switch
			m_StateMachines[CurrentStateMachine].ForceStateChange(iState);
		}

		/// <summary>
		/// The states have changed, go through and set all the actions of the new state to "not run"
		/// if the state change was not safe, we need to pop back into the previous state.
		/// </summary>
		/// <param name="iCurState">the new state of the object</param>
		public void StateChange(object sender, StateChangeEventArgs eventArgs)
		{
			//check if we are changing state machines
			switch (m_StateMachines[CurrentStateMachine].CurrentState())
			{
				case (int)EState.SwitchToGroundStateMachine:
				{
					//Try to change state machines if it is safe
					StateMachineIndex(0, eventArgs);
				}
				break;
				case (int)EState.SwitchToUpStateMachine:
				{
					StateMachineIndex(1, eventArgs);
				}
				break;
				default:
				{
					//reset the current single state container
					m_StateMachines[CurrentStateMachine].StateChange(sender, eventArgs);

					//for all other events, fire off the event if anyone is listening
					if (null != StateChangedEvent)
					{
						StateChangedEvent(this, eventArgs);
					}
				}
				break;
			}
		}

		/// <summary>
		/// change the index of the state machine to use
		/// </summary>
		/// <param name="iIndex"></param>
		protected void StateMachineIndex(int iIndex, StateChangeEventArgs eventArgs)
		{
			Debug.Assert(0 <= iIndex);
			Debug.Assert(iIndex < 4);

			//better not be switching to the same state machine...
			Debug.Assert(iIndex != CurrentStateMachine);

			//do a little timer so it doesn't pop back and forth between state machines...
			//but ignore it for switching into/out of ground state machine
			if ((0 == iIndex) || (0 == CurrentStateMachine) || (m_StateMachineChangeTimer.RemainingTime() <= 0.0f))
			{
				//reset the new state machine to the initial state
				m_StateMachines[CurrentStateMachine].Reset();

				//switch to new state machine, which will sign up for events etc
				CurrentStateMachine = iIndex;
				
				//reset the state timer
				m_StateMachineChangeTimer.Start(0.5f);

				//reset the single state container
				m_StateMachines[CurrentStateMachine].StateChange(iIndex, eventArgs);

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
				m_StateMachines[CurrentStateMachine].StateMachine.StateChangedEvent -= this.StateChange;

				//force the state of the old state machine
				m_StateMachines[CurrentStateMachine].ForceStateChange(eventArgs.OldState);

				//re-sign up for state change events of the old state machine
				m_StateMachines[CurrentStateMachine].StateMachine.StateChangedEvent += this.StateChange;
			}
		}

		/// <summary>
		/// Execute the actions for the current state
		/// </summary>
		/// <param name="fPrevTime">the last time that the object executed actions</param>
		/// <param name="fCurTime">the time in seconds that the object has been in this state</param>
		public void ExecuteActions(GameClock rGameClock)
		{
			Debug.Assert(null != m_StateMachines);
			Debug.Assert(CurrentStateMachine >= 0);
			Debug.Assert(CurrentStateMachine < m_StateMachines.Count);

			//we better not be running state actions in these states...
			Debug.Assert("SwitchToGroundStateMachine" != CurrentStateText());
			Debug.Assert("SwitchToUpStateMachine" != CurrentStateText());

			m_StateMachineChangeTimer.Update(rGameClock);
			m_StateMachines[CurrentStateMachine].ExecuteActions(rGameClock);
		}

		/// <summary>
		/// Check whether or not the current state is an attack state
		/// </summary>
		/// <returns></returns>
		public bool IsCurrentStateAttack()
		{
			Debug.Assert(null != m_StateMachines);
			Debug.Assert(CurrentStateMachine >= 0);
			Debug.Assert(CurrentStateMachine < m_StateMachines.Count);

			return m_StateMachines[CurrentStateMachine].IsCurrentStateAttack();
		}

		/// <summary>
		/// Check if a state is an attack state
		/// </summary>
		/// <param name="iState">The state to check if is an attack state</param>
		/// <returns>bool: whether or not the requested state has an attack</returns>
		public bool IsStateAttack(int iState)
		{
			Debug.Assert(null != m_StateMachines);
			Debug.Assert(CurrentStateMachine >= 0);
			Debug.Assert(CurrentStateMachine < m_StateMachines.Count);
			Debug.Assert(iState >= 0);

			return m_StateMachines[CurrentStateMachine].IsStateAttack(iState);
		}

		/// <summary>
		/// Check if an attack is active in this state.  Used to queue moves during a combo.
		/// </summary>
		/// <returns>bool: this state hasnt yet entered the recovery phase.  
		/// The move should be queued until it finishes.</returns>
		public bool IsAttackActive()
		{
			Debug.Assert(null != m_StateMachines);
			Debug.Assert(CurrentStateMachine >= 0);
			Debug.Assert(CurrentStateMachine < m_StateMachines.Count);

			return m_StateMachines[CurrentStateMachine].IsAttackActive();
		}

		/// <summary>
		/// Replace all the base object pointers in this dude to point to a replacement object
		/// </summary>
		/// <param name="myBot">the replacement dude</param>
		public void ReplaceOwner(BaseObject myBot)
		{
			Debug.Assert(null != m_StateMachines);

			//replace in all the state actions
			for (int i = 0; i < 4; i++)
			{
				m_StateMachines[i].ReplaceOwner(myBot);
			}
		}

		public int CurrentState()
		{
			Debug.Assert(null != m_StateMachines);
			Debug.Assert(CurrentStateMachine >= 0);
			Debug.Assert(CurrentStateMachine < m_StateMachines.Count);

			return m_StateMachines[CurrentStateMachine].CurrentState();
		}

		public int PrevState()
		{
			Debug.Assert(null != m_StateMachines);
			Debug.Assert(CurrentStateMachine >= 0);
			Debug.Assert(CurrentStateMachine < m_StateMachines.Count);

			return m_StateMachines[CurrentStateMachine].PrevState();
		}

		public int GetStateIndexFromText(string strStateName)
		{
			Debug.Assert(null != m_StateMachines);
			Debug.Assert(CurrentStateMachine >= 0);
			Debug.Assert(CurrentStateMachine < m_StateMachines.Count);
			Debug.Assert(m_StateMachines.Count > 0);

#if DEBUG
			//make sure the state machines all match
			int iIndex = m_StateMachines[0].GetStateIndexFromText(strStateName);
			for (int i = 0; i < m_StateMachines.Count; i++)
			{
				Debug.Assert(m_StateMachines[i].GetStateIndexFromText(strStateName) == iIndex);
			}
#endif
			return m_StateMachines[CurrentStateMachine].GetStateIndexFromText(strStateName);
		}

		public int GetMessageIndexFromText(string strMessageName)
		{
			Debug.Assert(null != m_StateMachines);
			Debug.Assert(CurrentStateMachine >= 0);
			Debug.Assert(CurrentStateMachine < m_StateMachines.Count);

#if DEBUG
			//make sure the state machines all match
			int iIndex = m_StateMachines[0].GetMessageIndexFromText(strMessageName);
			for (int i = 0; i < m_StateMachines.Count; i++)
			{
				Debug.Assert(m_StateMachines[i].GetMessageIndexFromText(strMessageName) == iIndex);
			}
#endif
			return m_StateMachines[CurrentStateMachine].GetMessageIndexFromText(strMessageName);
		}

		public string GetStateName(int iStateIndex)
		{
			Debug.Assert(null != m_StateMachines);
			Debug.Assert(CurrentStateMachine >= 0);
			Debug.Assert(CurrentStateMachine < m_StateMachines.Count);

#if DEBUG
			//make sure the state machines all match
			string strName = m_StateMachines[0].GetStateName(iStateIndex);
			for (int i = 1; i < m_StateMachines.Count; i++)
			{
				Debug.Assert(m_StateMachines[i].GetStateName(iStateIndex) == strName);
			}
#endif
			return m_StateMachines[CurrentStateMachine].GetStateName(iStateIndex);
		}

		public string GetMessageName(int iMessageIndex)
		{
			Debug.Assert(null != m_StateMachines);
			Debug.Assert(CurrentStateMachine >= 0);
			Debug.Assert(CurrentStateMachine < m_StateMachines.Count);

#if DEBUG
			//make sure the state machines all match
			string strName = m_StateMachines[0].GetMessageName(iMessageIndex);
			for (int i = 1; i < 4; i++)
			{
				Debug.Assert(m_StateMachines[i].GetMessageName(iMessageIndex) == strName);
			}
#endif
			return m_StateMachines[CurrentStateMachine].GetMessageName(iMessageIndex);
		}

		public int NumStates()
		{
			Debug.Assert(null != m_StateMachines);
			Debug.Assert(CurrentStateMachine >= 0);
			Debug.Assert(CurrentStateMachine < m_StateMachines.Count);

			//find the state machine with the most states
			int iNumStates = m_StateMachines[0].NumStates();
			for (int i = 1; i < m_StateMachines.Count; i++)
			{
				if (m_StateMachines[i].NumStates() > iNumStates)
				{
					iNumStates = m_StateMachines[i].NumStates();
				}
			}

			return iNumStates;
		}

		public int NumMessages()
		{
			Debug.Assert(null != m_StateMachines);
			Debug.Assert(CurrentStateMachine >= 0);
			Debug.Assert(CurrentStateMachine < m_StateMachines.Count);

			//find the state machine with the most states
			int iNumMessages = m_StateMachines[0].NumMessages();
			for (int i = 1; i < 4; i++)
			{
				if (m_StateMachines[i].NumMessages() > iNumMessages)
				{
					iNumMessages = m_StateMachines[i].NumMessages();
				}
			}

			return iNumMessages;
		}

		public StateActions GetStateActions(int iStateIndex)
		{
			Debug.Assert(null != m_StateMachines);
			Debug.Assert(CurrentStateMachine >= 0);
			Debug.Assert(CurrentStateMachine < m_StateMachines.Count);

			return m_StateMachines[CurrentStateMachine].GetStateActions(iStateIndex);
		}

		public GameClock GetStateClock()
		{
			Debug.Assert(null != m_StateMachines);
			Debug.Assert(CurrentStateMachine >= 0);
			Debug.Assert(CurrentStateMachine < m_StateMachines.Count);

			return m_StateMachines[CurrentStateMachine].GetStateClock();
		}

		public string CurrentStateText()
		{
			return m_StateMachines[CurrentStateMachine].CurrentStateText();
		}

		#endregion //Methods

		#region Networking

#if NETWORKING

		/// <summary>
		/// Read this object from a network packet reader.
		/// </summary>
		public void ReadFromNetwork(PacketReader packetReader)
		{
			int iIndex = packetReader.ReadInt32();

			if (CurrentStateMachine != iIndex)
			{
				CurrentStateMachine = iIndex;
			}

			Debug.Assert(null != m_StateMachines);
			Debug.Assert(CurrentStateMachine >= 0);
			Debug.Assert(CurrentStateMachine < m_StateMachines.Count);

			m_StateMachines[CurrentStateMachine].ReadFromNetwork(packetReader);
		}

		/// <summary>
		/// Write this object to a network packet reader.
		/// </summary>
		public void WriteToNetwork(PacketWriter packetWriter)
		{
			Debug.Assert(null != m_StateMachines);
			Debug.Assert(CurrentStateMachine >= 0);
			Debug.Assert(CurrentStateMachine < m_StateMachines.Count);

			packetWriter.Write(CurrentStateMachine);
			m_StateMachines[CurrentStateMachine].WriteToNetwork(packetWriter);
		}

#endif

		#endregion //Networking

		#region State Action File IO

		public bool ReadXml(string strFilename, BaseObject rOwner, IGameDonkey rEngine)
		{
			//don't call this dude, he'll set his own shit up
			Debug.Assert(false);
			return false;
		}

		public bool WriteXml(string strFilename)
		{
			if (m_StateMachines.Count > 0)
			{
				return m_StateMachines[0].WriteXml(strFilename);
			}

			return false;
		}

		public void ReadSerialized(ContentManager rXmlContent,
			string strResource,
			BaseObject rOwner,
			IGameDonkey rEngine)
		{
			//don't call this dude, he'll set his own shit up
		}

		#endregion //State Action File IO

		#region State Machine File IO

		public bool ReadXmlStateMachineFile(string strFilename)
		{
			//not implemented
			Debug.Assert(false);
			return false;
		}

		public bool AppendXmlStateMachineFile(string strFilename)
		{
			//not implemented
			Debug.Assert(false);
			return false;
		}

		public bool ReadSerializedStateMachineFile(ContentManager rContent, string strResource, int iMessageOffset)
		{
			//not implemented
			Debug.Assert(false);
			return false;
		}

		public bool AppendSerializedStateMachineFile(ContentManager rContent, string strResource, int iMessageOffset)
		{
			//not implemented
			Debug.Assert(false);
			return false;
		}

		#endregion //File IO

		#region Combined File IO

		public bool ReadXmlStateContainer(string strStateMachineFilename,
			int iMessageOffset,
			string strStateActionsFilename,
			BaseObject rOwner,
			IGameDonkey rEngine,
			bool bPlayerStateMachine,
			bool bFlyingStateMachine)
		{
			//create a new single state container
			SingleStateContainer rMyStateContainer = new SingleStateContainer(new WeddingStateMachine(bFlyingStateMachine), "UpStates");

			//find a place to store the new state container
			m_StateMachines.Add(rMyStateContainer);
			Debug.Assert(m_StateMachines.Count < 4);

			//read in the stuff
			if (!rMyStateContainer.ReadXmlStateContainer(
				strStateMachineFilename, 
				iMessageOffset, 
				strStateActionsFilename, 
				rOwner, 
				rEngine,
				bPlayerStateMachine, 
				bFlyingStateMachine))
			{
				return false;
			}

			return true;
		}

		public void ReadSerializedStateContainer(ContentManager rContent,
			string strStateMachineResource,
			int iMessageOffset,
			string strStateActionsResource,
			BaseObject rOwner,
			IGameDonkey rEngine,
			bool bPlayerStateMachine,
			bool bFlyingStateMachine)
		{
			//create a new single state container
			SingleStateContainer rMyStateContainer = new SingleStateContainer(new WeddingStateMachine(bFlyingStateMachine), "GroundStates");

			//find a place to store the new state container
			m_StateMachines.Add(rMyStateContainer);

			//read in the stuff
			rMyStateContainer.ReadSerializedStateContainer(rContent, 
				strStateMachineResource, 
				iMessageOffset, 
				strStateActionsResource, 
				rOwner, 
				rEngine, 
				bPlayerStateMachine, 
				bFlyingStateMachine);
		}

		#endregion //Combined File IO
	}
}