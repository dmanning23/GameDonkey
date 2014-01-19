using System.Collections.Generic;
using System.Xml;
using GameTimer;
using System.Diagnostics;
using Microsoft.Xna.Framework.Content;
#if NETWORKING
using Microsoft.Xna.Framework.Net;
#endif
using StateMachineBuddy;
using System;
using FilenameBuddy;
using SPFSettings;

namespace GameDonkey
{
	/// <summary>
	/// This is a container with multiple hierarchical state machines
	/// </summary>
	public class PlayerObjectStateContainer : IStateContainer
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
					m_StateMachines[_currentStateMachine].StateMachine.StateChangedEvent -= StateChange;
					_currentStateMachine = value;
					m_StateMachines[_currentStateMachine].StateMachine.StateChangedEvent += StateChange;
				}
			}
		}

		private bool IsCurrentStateMachineValid
		{
			get
			{
				return (_currentStateMachine < m_StateMachines.Count);
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

		/// <summary>
		/// Flag for whether or not we want to change bewteen state machines.
		/// Used in the state machine tool to work on one container at a time.
		/// </summary>
		public bool IgnoreStateMachineChange { get; set; }

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
			IgnoreStateMachineChange = false;
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

			//Dont reset the state container if in toool mode
			if (!IgnoreStateMachineChange)
			{
				CurrentStateMachine = 0;
			}

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
			Debug.Assert(CurrentStateMachine >= 0);
			Debug.Assert(CurrentStateMachine < m_StateMachines.Count);

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
		public virtual void StateChange(object sender, StateChangeEventArgs eventArgs)
		{
			//reset the current single state container
			m_StateMachines[CurrentStateMachine].StateChange(sender, eventArgs);

			//for all other events, fire off the event if anyone is listening
			if (null != StateChangedEvent)
			{
				StateChangedEvent(this, eventArgs);
			}
		}

		/// <summary>
		/// change the index of the state machine to use
		/// </summary>
		/// <param name="iIndex"></param>
		public void StateMachineIndex(int iIndex, StateChangeEventArgs eventArgs)
		{
			Debug.Assert(0 <= iIndex);
			Debug.Assert(iIndex < m_StateMachines.Count);

			//do a little timer so it doesn't pop back and forth between state machines...
			//but ignore it for switching into/out of ground state machine
			if (!IgnoreStateMachineChange && //Is the flag set to ignore state container changes?
				((0 == iIndex) || 
				(0 == CurrentStateMachine) || 
				(m_StateMachineChangeTimer.RemainingTime() <= 0.0f)))
			{
				//better not be switching to the same state machine...
				Debug.Assert(iIndex != CurrentStateMachine);

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
		public virtual void ExecuteActions(GameClock rGameClock)
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
			for (int i = 0; i < m_StateMachines.Count; i++)
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
			for (int i = 1; i < m_StateMachines.Count; i++)
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

			return m_StateMachines[CurrentStateMachine].NumStates();
		}

		public int NumMessages()
		{
			Debug.Assert(null != m_StateMachines);
			Debug.Assert(CurrentStateMachine >= 0);
			Debug.Assert(CurrentStateMachine < m_StateMachines.Count);

			//find the state machine with the most states
			int iNumMessages = m_StateMachines[0].NumMessages();
			for (int i = 1; i < m_StateMachines.Count; i++)
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

		public override string ToString()
		{
			return Name;
		}

		public int StateMachineIndex()
		{
			return 0;
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

		#region File IO

		public virtual bool ReadXmlStateContainer(BaseObjectData xmlData, IGameDonkey rEngine, int iMessageOffset, BaseObject rOwner)
		{
			//don't call this dude, he'll set his own shit up
			Debug.Assert(false);
			return false;
		}

		public bool WriteXml()
		{
			bool bOk = false;
			foreach (var container in m_StateMachines)
			{
				if (!container.WriteXml())
				{
					bOk = false;
				}
			}

			return bOk;
		}

		public virtual void ReadSerializedStateContainer(ContentManager rContent,
			List<StateContainerXML> childNodes,
			IGameDonkey rEngine,
			int iMessageOffset,
			BaseObject rOwner)
		{
			//don't call this dude, he'll set his own shit up
		}

		#endregion //File IO
	}
}