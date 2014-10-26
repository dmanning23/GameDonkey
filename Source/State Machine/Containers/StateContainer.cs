using GameTimer;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;
using System;
using System.Collections.Generic;
using SPFSettings;

namespace GameDonkey
{
	/// <summary>
	/// An interface for state machine and all the state actions for that machine.
	/// </summary>
	public interface IStateContainer
	{
		#region Events

		/// <summary>
		/// Event raised when current state changes
		/// </summary>
		event EventHandler<StateChangeEventArgs> StateChangedEvent;

		#endregion //Events

		#region Properties

		/// <summary>
		/// Get the number of containers, if this is a collection
		/// </summary>
		int NumContainers { get; }

		/// <summary>
		/// Get a list of all the containers in this dude.
		/// </summary>
		List<IStateContainer> Containers { get; }

		/// <summary>
		/// Get the name of this state container
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Get the current state machine for this container
		/// </summary>
		StateMachine StateMachine { get; }

		/// <summary>
		/// The index of the state machine currently being used
		/// </summary>
		int CurrentStateMachine { get; set; }

		/// <summary>
		/// Flag for whether or not we want to change bewteen state machines.
		/// Used in the state machine tool to work on one container at a time.
		/// </summary>
		bool IgnoreStateMachineChange { get; set; }

		#endregion //Properties

		#region Methods

		void Reset();

		/// <summary>
		/// method to send a message
		/// </summary>
		/// <param name="iMessage">message to send to the state machine, 
		/// should be offset by the message offset of this dude</param>
		void SendStateMessage(int iMessage);

		void ForceStateChange(int iState);

		/// <summary>
		/// The states have changed, go through and set all the actions of the new state to "not run"
		/// </summary>
		/// <param name="iCurState">the new state of the object</param>
		void StateChange(object sender, StateChangeEventArgs e);

		/// <summary>
		/// Execute the actions for the current state
		/// </summary>
		/// <param name="fPrevTime">the last time that the object executed actions</param>
		/// <param name="fCurTime">the time in seconds that the object has been in this state</param>
		void ExecuteActions(GameClock rGameClock);

		/// <summary>
		/// Check whether or not the current state is an attack state
		/// </summary>
		/// <returns></returns>
		bool IsCurrentStateAttack();

		/// <summary>
		/// Check if a state is an attack state
		/// </summary>
		/// <param name="iState">The state to check if is an attack state</param>
		/// <returns>bool: whether or not the requested state has an attack</returns>
		bool IsStateAttack(int iState);

		/// <summary>
		/// Check if an attack is active in this state.  Used to queue moves during a combo.
		/// </summary>
		/// <returns>bool: this state hasnt yet entered the recovery phase.  
		/// The move should be queued until it finishes.</returns>
		bool IsAttackActive();

		/// <summary>
		/// Replace all the base object pointers in this dude to point to a replacement object
		/// </summary>
		/// <param name="myBot">the replacement dude</param>
		void ReplaceOwner(BaseObject myBot);

		int CurrentState();

		int PrevState();

		int GetStateIndexFromText(string strStateName);

		int GetMessageIndexFromText(string strMessageName);

		string GetStateName(int iStateIndex);

		string GetMessageName(int iMessageIndex);

		int NumStates();

		int NumMessages();

		string CurrentStateText();

		StateActions GetStateActions(int iStateIndex);

		GameClock GetStateClock();

		#endregion //Methods

		#region File IO

		bool ReadXmlStateContainer(BaseObjectData xmlData, IGameDonkey rEngine, int iMessageOffset, BaseObject rOwner);

		bool WriteXml();

		void ReadSerializedStateContainer(ContentManager rContent, List<StateContainerXML> childNodes, IGameDonkey rEngine, int iMessageOffset, BaseObject rOwner);

		#endregion //File IO
	}
}