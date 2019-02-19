using GameTimer;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;
using System;
using System.Collections.Generic;

namespace GameDonkeyLib
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
		event EventHandler<HybridStateChangeEventArgs> StateChangedEvent;

		#endregion //Events

		#region Properties

		/// <summary>
		/// Get the number of containers, if this is a collection
		/// </summary>
		int NumContainers { get; }

		StateMachineActions Actions { get; }

		/// <summary>
		/// Get a list of all the containers in this dude.
		/// </summary>
		List<IStateContainer> StateContainers { get; }

		/// <summary>
		/// Get the name of this state container
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Get the current state machine for this container
		/// </summary>
		HybridStateMachine StateMachine { get; }

		/// <summary>
		/// The index of the state machine currently being used
		/// </summary>
		int CurrentStateMachine { get; set; }

		/// <summary>
		/// Flag for whether or not we want to change bewteen state machines.
		/// Used in the state machine tool to work on one container at a time.
		/// </summary>
		bool IgnoreStateMachineChange { get; set; }

		string CurrentState { get; }

		string PrevState { get; }

		GameClock StateClock { get; }

		#endregion //Properties

		#region Methods

		void Reset();

		/// <summary>
		/// method to send a message
		/// </summary>
		/// <param name="message">message to send to the state machine, 
		/// should be offset by the message offset of this dude</param>
		void SendStateMessage(string message);

		void ForceStateChange(string state);

		/// <summary>
		/// The states have changed, go through and set all the actions of the new state to "not run"
		/// </summary>
		/// <param name="iCurState">the new state of the object</param>
		void StateChange(object sender, HybridStateChangeEventArgs e);

		/// <summary>
		/// Execute the actions for the current state
		/// </summary>
		void ExecuteActions(GameClock clock);

		/// <summary>
		/// Check whether or not the current state is an attack state
		/// </summary>
		/// <returns></returns>
		bool IsCurrentStateAttack();

		/// <summary>
		/// Check if a state is an attack state
		/// </summary>
		/// <param name="state">The state to check if is an attack state</param>
		/// <returns>bool: whether or not the requested state has an attack</returns>
		bool IsStateAttack(string state);

		/// <summary>
		/// Check if an attack is active in this state.  Used to queue moves during a combo.
		/// </summary>
		/// <returns>bool: this state hasnt yet entered the recovery phase.  
		/// The move should be queued until it finishes.</returns>
		bool IsAttackActive();

		/// <summary>
		/// Replace all the base object pointers in this dude to point to a replacement object
		/// </summary>
		/// <param name="bot">the replacement dude</param>
		void ReplaceOwner(BaseObject bot);

		SingleStateActions GetStateActions(string stateName);

		void LoadContent(BaseObjectModel baseObjectmodel, BaseObject owner, IGameDonkey engine, ContentManager content);

		void WriteXml();

		#endregion //Methods
	}
}