using System.Diagnostics;
using GameTimer;
using Microsoft.Xna.Framework.Content;
#if NETWORKING
using Microsoft.Xna.Framework.Net;
#endif
using StateMachineBuddy;
using System;

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
		event EventHandler<StateChangeEventArgs> StateChangedContainerEvent;

		#endregion //Events

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

		CStateActions GetStateActions(int iStateIndex);

		GameClock GetStateClock();

		#endregion //Methods

		#region Networking

#if NETWORKING

		/// <summary>
		/// Read this object from a network packet reader.
		/// </summary>
		void ReadFromNetwork(PacketReader packetReader);

		/// <summary>
		/// Write this object to a network packet reader.
		/// </summary>
		void WriteToNetwork(PacketWriter packetWriter);

#endif

		#endregion //Networking

		#region State Action File IO

		bool ReadSerializedStateActions(string strFilename, BaseObject rOwner, IGameDonkey rEngine);

		bool WriteStateActions(string strFilename);

		void ReadSerializedStateActions(ContentManager rXmlContent,
			string strResource,
			BaseObject rOwner,
			IGameDonkey rEngine);

		#endregion //State Action File IO

		#region State Machine File IO

		bool ReadStateMachineFile(string strFilename);

		bool AppendStateMachineFile(string strFilename);

		bool ReadStateMachineFile(ContentManager rContent, string strResource, int iMessageOffset);

		bool AppendStateMachineFile(ContentManager rContent, string strResource, int iMessageOffset);

		#endregion //File IO

		#region Combined File IO

		bool ReadStateContainer(string strStateMachineFilename, 
			int iMessageOffset,
			string strStateActionsFilename,
			BaseObject rOwner,
			IGameDonkey rEngine,
			bool bPlayerStateMachine,
			bool bFlyingStateMachine);

		void ReadStateContainer(ContentManager rContent, 
			string strStateMachineResource, 
			int iMessageOffset,
			string strStateActionsResource,
			BaseObject rOwner, 
			IGameDonkey rEngine,
			bool bPlayerStateMachine,
			bool bFlyingStateMachine);

		#endregion //Combined File IO
	}
}