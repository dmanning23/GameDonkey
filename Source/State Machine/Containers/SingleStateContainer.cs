using FilenameBuddy;
using GameTimer;
using Microsoft.Xna.Framework.Content;
using SPFSettings;
using StateMachineBuddy;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameDonkey
{
	/// <summary>
	/// This a state container with one state machine
	/// Heads up, this state machine doesn't sign up for all the state change messages...
	/// It's meant to be used with playerstatecontainer, otherwise use the singlestatecontainer
	/// </summary>
	public class SingleStateContainer : IStateContainer
	{
		#region Members

		/// <summary>
		/// Occurs when the state changes in the state machine.
		/// </summary>
		public event EventHandler<StateChangeEventArgs> StateChangedEvent;

		/// <summary>
		/// Get the current state machine for this container
		/// </summary>
		public StateMachine StateMachine { get; private set; }

		private StateActionsList m_listActions;

		/// <summary>
		/// This clock times how long the character has been in the current state
		/// </summary>
		protected GameClock m_StateClock;

		/// <summary>
		/// the filename where the state actions go
		/// </summary>
		private Filename m_strActionsFile;

		/// <summary>
		/// Flag for whether or not we want to change bewteen state machines.
		/// Used in the state machine tool to work on one container at a time.
		/// Doesn't really do shit in a single container.
		/// </summary>
		public bool IgnoreStateMachineChange { get; set; }

		#endregion //Members

		#region Properties

		/// <summary>
		/// Get the number of containers, if this is a collection
		/// </summary>
		public int NumContainers
		{
			get
			{ 
				//Single state container, get it?
				return 1; 
			}
		}

		/// <summary>
		/// Get a list of all the containers in this dude.
		/// </summary>
		public List<IStateContainer> Containers 
		{
			get
			{
				return new List<IStateContainer> { this };
			}
		}

		/// <summary>
		/// Get the name of this state container
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The index of the state machine currently being used
		/// </summary>
		public int CurrentStateMachine 
		{
			get
			{
				//there's only one!
				return 0;
			}
			set
			{
				//wtf?
			}
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// contructor
		/// </summary>
		/// <param name="myStateMachine">the state machine this dude will use</param>
		public SingleStateContainer(StateMachine myStateMachine, string containerName)
		{
			StateMachine = myStateMachine;
			m_listActions = new StateActionsList();
			m_StateClock = new GameClock();

			//This container only signs up for the reset event
			StateMachine.ResetEvent += this.StateChange;
			Name = containerName;
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
			if (null != StateChangedEvent)
			{
				StateChangedEvent(this, eventArgs);
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

		public StateActions GetStateActions(int iStateIndex)
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

		public override string ToString()
		{
			return Name;
		}

		#endregion //Methods

		#region State Action File IO

		/// <summary>
		/// Read the list of state contiainers:
		/// <states>
		///		<Item Type="SPFSettings.StateContainerXML">
		///			<stateMachine>state machine.xml</stateMachine>
		///			<stateContainer>state actions.xml</stateContainer>
		///		</Item>
		///	</states>
		/// </summary>
		/// <param name="xmlData"></param>
		/// <param name="rEngine"></param>
		/// <param name="iMessageOffset"></param>
		/// <param name="rOwner"></param>
		/// <returns></returns>
		public bool ReadXmlStateContainer(BaseObjectData xmlData, IGameDonkey rEngine, int iMessageOffset, BaseObject rOwner)
		{
			//Get that first node
			foreach (var singleData in xmlData.StateContainers)
			{
				if (!ReadXmlSingleStateContainer(singleData, rEngine, iMessageOffset, rOwner))
				{
					Debug.Assert(false);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Read in a single state container
		///	<Item Type="SPFSettings.StateContainerXML">
		///		<stateMachine>state machine.xml</stateMachine>
		///		<stateContainer>state actions.xml</stateContainer>
		///	</Item>
		/// </summary>
		/// <param name="rXMLNode"></param>
		/// <param name="rEngine"></param>
		/// <param name="iMessageOffset"></param>
		/// <param name="rOwner"></param>
		/// <returns></returns>
		public bool ReadXmlSingleStateContainer(StateContainerXML rXMLNode, IGameDonkey rEngine, int iMessageOffset, BaseObject rOwner)
		{
			//load the state machine
			if (!ReadXmlStateMachine(StateMachine, new Filename(rXMLNode.stateMachine)))
			{
				Debug.Assert(false);
				return false;
			}

			//get the state action xml node

			//load the state actions
			Debug.Assert(null != m_listActions);
			m_strActionsFile = new Filename(rXMLNode.stateActions);
			return m_listActions.ReadXmlStateActions(m_strActionsFile, rOwner, rEngine, this);
		}

		public virtual bool ReadXmlStateMachine(StateMachine machine, Filename file)
		{
			return machine.ReadXmlFile(file);
		}

		public bool WriteXml()
		{
			return m_listActions.WriteXml(m_strActionsFile, StateMachine);
		}

		#endregion //Combined File IO
	}
}