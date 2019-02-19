using FilenameBuddy;
using GameTimer;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;
using System;
using System.Collections.Generic;
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
		#region Properties

		/// <summary>
		/// the filename where the state actions go
		/// </summary>
		private Filename StateContainerFilename;

		private Filename StateMachineFilename;

		/// <summary>
		/// Occurs when the state changes in the state machine.
		/// </summary>
		public event EventHandler<HybridStateChangeEventArgs> StateChangedEvent;

		/// <summary>
		/// Get the current state machine for this container
		/// </summary>
		public HybridStateMachine StateMachine { get; private set; }

		public StateMachineActions Actions { get; private set; }

		/// <summary>
		/// Flag for whether or not we want to change bewteen state machines.
		/// Used in the state machine tool to work on one container at a time.
		/// Doesn't really do shit in a single container.
		/// </summary>
		public bool IgnoreStateMachineChange { get; set; }

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

		public string CurrentState
		{
			get
			{
				return StateMachine.CurrentState;
			}
		}

		public string PrevState
		{
			get
			{
				return StateMachine.PrevState;
			}
		}

		/// <summary>
		/// This clock times how long the character has been in the current state
		/// </summary>
		public GameClock StateClock { get; protected set; }

		#endregion //Properties

		#region Initialization

		public SingleStateContainer(HybridStateMachine stateMachine, string containerName)
		{
			StateMachine = stateMachine;
			Actions = new StateMachineActions();
			StateClock = new GameClock();

			StateMachine.ResetEvent += this.StateChange;
			Name = containerName;

			StateContainerFilename = new Filename();
			StateMachineFilename = new Filename();
		}

		public void LoadContent(BaseObjectModel baseObjectmodel, BaseObject owner, IGameDonkey engine, ContentManager content)
		{
			LoadContent(baseObjectmodel.States.FirstOrDefault(), owner, engine, content);
		}

		public void LoadContent(StateContainerModel stateContainerModel, BaseObject owner, IGameDonkey engine, ContentManager content)
		{
			//grab the filenames
			StateContainerFilename = new Filename(stateContainerModel.StateActionsFilename);
			StateMachineFilename = new Filename(stateContainerModel.StateMachineFilename);

			//load the state machine
			LoadStateMachine(StateMachine, StateMachineFilename, content);

			//Create the statecontainer model and load it
			using (var singleStateContainerModel = new SingleStateContainerModel(StateContainerFilename))
			{
				singleStateContainerModel.ReadXmlFile(content);

				//load the actions into the statemachine actions
				LoadContainer(singleStateContainerModel, owner);
			}

			//load the action content
			Actions.LoadContent(engine, content);
		}

		protected virtual void LoadContainer(SingleStateContainerModel stateContainerModel, BaseObject owner)
		{
			//load into the statemachineactions object
			Actions.LoadStateActions(StateMachine, stateContainerModel, owner);
		}

		public virtual void LoadStateMachine(HybridStateMachine machine, Filename file, ContentManager content)
		{
			if (file.HasFilename)
			{
				//Load the state machine model
				using (var model = new StateMachineModel(file))
				{
					model.ReadXmlFile(content);
					machine.AddStateMachine(model);

					machine.SetInitialState(model.Initial);
				}
			}
		}

		public void WriteXml()
		{
			//create the model
			using (var model = new SingleStateContainerModel(StateContainerFilename, this))
			{
				//write the model out
				model.WriteXml();
			}

			//write out the state machine
			if (StateMachineFilename.HasFilename)
			{
				using (var model = new StateMachineModel(StateMachineFilename, StateMachine))
				{
					//write the model out
					model.WriteXml();
				}
			}
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
		public void SendStateMessage(string message)
		{
			StateMachine.SendStateMessage(message);
		}

		public void ForceStateChange(string state)
		{
			StateMachine.ForceState(state);
		}

		/// <summary>
		/// The states have changed, go through and set all the actions of the new state to "not run"
		/// </summary>
		/// <param name="iCurState">the new state of the object</param>
		public void StateChange(object sender, HybridStateChangeEventArgs eventArgs)
		{
			//set the new state actions to 'not run'
			Actions.StateChange(eventArgs.NewState);

			//restart the state clock
			StateClock.Start();
			StateClock.TimeDelta = 0.0f;

			//fire off the event if anyone is listening
			StateChangedEvent?.Invoke(this, eventArgs);
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
		public bool IsStateAttack(string state)
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

		public override string ToString()
		{
			return Name;
		}

		public SingleStateActions GetStateActions(string stateName)
		{
			return Actions.GetStateActions(stateName);
		}

		#endregion //Methods
	}
}