using FilenameBuddy;
using GameTimer;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GameDonkeyLib
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
		/// the filename where the state actions go
		/// </summary>
		private Filename StateContainerFilename;

		private Filename StateMachineFilename;

		/// <summary>
		/// Occurs when the state changes in the state machine.
		/// </summary>
		public event EventHandler<StateChangeEventArgs> StateChangedEvent;

		/// <summary>
		/// Get the current state machine for this container
		/// </summary>
		public StateMachine StateMachine { get; private set; }

		public StateMachineActions Actions { get; private set; }

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
		public List<IStateContainer> StateContainers
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

		public int CurrentState
		{
			get
			{
				return StateMachine.CurrentState;
			}
		}

		public int PrevState
		{
			get
			{
				return StateMachine.PrevState;
			}
		}

		public int NumStates
		{
			get
			{
				return StateMachine.NumStates;
			}
		}

		public int NumMessages
		{
			get
			{
				return StateMachine.NumMessages;
			}
		}

		/// <summary>
		/// This clock times how long the character has been in the current state
		/// </summary>
		public GameClock StateClock { get; protected set; }

		public string CurrentStateText
		{
			get
			{
				return StateMachine.CurrentStateText;
			}
		}

		#endregion //Properties

		#region Initialization

		public SingleStateContainer(StateMachine stateMachine, string containerName)
		{
			StateMachine = stateMachine;
			Actions = new StateMachineActions();
			StateClock = new GameClock();

			//This container only signs up for the reset event
			StateMachine.ResetEvent += this.StateChange;
			Name = containerName;

			StateContainerFilename = new Filename();
			StateMachineFilename = new Filename();
		}

		public void LoadContent(BaseObjectModel baseObjectmodel, BaseObject owner, IGameDonkey engine, int messageOffset, ContentManager content)
		{
			//Get that first node
			var stateContainerModel = baseObjectmodel.States.FirstOrDefault();

			//grab the filenames
			StateContainerFilename = new Filename(stateContainerModel.StateActionsFilename);
			StateMachineFilename = new Filename(stateContainerModel.StateMachineFilename);

			//load the state machine
			StateMachine.MessageOffset = messageOffset;
			LoadStateMachine(StateMachine, StateMachineFilename, content);

			//Create the statecontainer model and load it
			var singleStateContainerModel = new SingleStateContainerModel(StateContainerFilename);
			singleStateContainerModel.ReadXmlFile(content);

			//load the actions into the statemachine actions
			LoadContainer(singleStateContainerModel, owner);

			//load the action content
			Actions.LoadContent(engine, this, content);
		}

		protected virtual void LoadContainer(SingleStateContainerModel stateContainerModel, BaseObject owner)
		{
			//load into the statemachineactions object
			Actions.LoadStateActions(stateContainerModel, owner);
		}

		public virtual void LoadStateMachine(StateMachine machine, Filename file, ContentManager content)
		{
			machine.LoadXml(file, content);
		}

		#endregion //Initialization

		#region Methods



		public void Reset()
		{
			StateMachine.ResetToInitialState();
		}

		/// <summary>
		/// method to send a message
		/// </summary>
		/// <param name="message">message to send to the state machine, 
		/// should be offset by the message offset of this dude</param>
		/// <returns>bool: did it change states?</returns>
		public void SendStateMessage(int message)
		{
			StateMachine.SendStateMessage(message);
		}

		public void ForceStateChange(int state)
		{
			StateMachine.ForceState(state);
		}

		/// <summary>
		/// The states have changed, go through and set all the actions of the new state to "not run"
		/// </summary>
		/// <param name="iCurState">the new state of the object</param>
		public void StateChange(object sender, StateChangeEventArgs eventArgs)
		{
			//set the new state actions to 'not run'
			Actions.StateChange(eventArgs.NewState);

			//restart the state clock
			StateClock.Start();
			StateClock.TimeDelta = 0.0f;

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
		public void ExecuteActions(GameClock gameClock)
		{
			StateClock.Update(gameClock);

			//execute the correct action container
			Actions.ExecuteActions(StateClock, StateMachine.CurrentState);
		}

		/// <summary>
		/// Check whether or not the current state is an attack state
		/// </summary>
		/// <returns></returns>
		public bool IsCurrentStateAttack()
		{
			return IsStateAttack(StateMachine.CurrentState);
		}

		/// <summary>
		/// Check if a state is an attack state
		/// </summary>
		/// <param name="state">The state to check if is an attack state</param>
		/// <returns>bool: whether or not the requested state has an attack</returns>
		public bool IsStateAttack(int state)
		{
			return Actions.IsStateAttack(state);
		}

		/// <summary>
		/// Check if an attack is active in this state.  Used to queue moves during a combo.
		/// </summary>
		/// <returns>bool: this state hasnt yet entered the recovery phase.  
		/// The move should be queued until it finishes.</returns>
		public bool IsAttackActive()
		{
			//check if the current state is an attack state, and if an attack is active
			return Actions.IsAttackActive(StateClock, StateMachine.CurrentState);
		}

		/// <summary>
		/// Replace all the base object pointers in this dude to point to a replacement object
		/// </summary>
		/// <param name="bot">the replacement dude</param>
		public void ReplaceOwner(BaseObject bot)
		{
			//replace in all the state actions
			Actions.ReplaceOwner(bot);
		}

		public int GetStateIndexFromText(string stateName)
		{
			return StateMachine.GetStateFromName(stateName);
		}

		public int GetMessageIndexFromText(string messageName)
		{
			return StateMachine.GetMessageFromName(messageName);
		}

		public string GetStateName(int stateIndex)
		{
			return StateMachine.GetStateName(stateIndex);
		}

		public string GetMessageName(int messageIndex)
		{
			return StateMachine.GetMessageName(messageIndex);
		}

		public StateActions GetStateActions(int stateIndex)
		{
			return Actions.GetStateActions(stateIndex);
		}

		public override string ToString()
		{
			return Name;
		}

		#endregion //Methods
	}
}