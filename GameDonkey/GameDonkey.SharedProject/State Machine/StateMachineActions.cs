using GameTimer;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;
using System.Collections.Generic;

namespace GameDonkeyLib
{
	/// <summary>
	/// A list of all the state actions for a single state machine.
	/// </summary>
	public class StateMachineActions
	{
		#region Properties

		public List<StateActions> Actions { get; private set; }

		#endregion //Properties

		#region Initialization

		/// <summary>
		/// standard constructor
		/// </summary>
		public StateMachineActions()
		{
			Actions = new List<StateActions>();
		}

		public void LoadStateActions(StateMachine stateMachine, SingleStateContainerModel stateContainerModel, BaseObject owner)
		{
			for (int i = 0; i < stateContainerModel.StatesActions.Count; i++)
			{
				StateActions actions = new StateActions();
				actions.LoadStateActions(stateContainerModel.StatesActions[i], owner);
				Actions.Add(actions);
			}

			if (Actions.Count != stateMachine.NumStates)
			{
				//Add state actions for any missing states
				for (int i = 0; i < stateMachine.NumStates; i++)
				{
					//get the state name
					var stateName = stateMachine.GetStateName(i);

					//make sure this thing already contains the states for it
					bool found = false;
					for (int j = 0; j < Actions.Count; j++)
					{
						if (Actions[j].StateName == stateName)
						{
							found = true;
							break;
						}
					}

					//add the actions if they werent found
					if (!found)
					{
						StateActions actions = new StateActions()
						{
							StateName = stateName
						};
						Actions.Add(actions);
					}
				}
			}
		}

		public void LoadContent(IGameDonkey engine, SingleStateContainer stateContainer, ContentManager content)
		{
			for (int i = 0; i < Actions.Count; i++)
			{
				Actions[i].LoadContent(engine, stateContainer, content);
			}
		}

		#endregion //Initialization

		#region Methods

		/// <summary>
		/// The states have changed, go through and set all the actions of the new state to "not run"
		/// </summary>
		/// <param name="nextState">the new state of the object</param>
		public void StateChange(int nextState)
		{
			//set the new state actions to 'not run'
			Actions[nextState].StateChange();
		}

		/// <summary>
		/// Execute the actions for the current state
		/// </summary>
		/// <param name="fPrevTime">the last time that the object executed actions</param>
		/// <param name="fCurTime">the time in seconds that the object has been in this state</param>
		public void ExecuteActions(GameClock clock, int currentState)
		{
			//execute the correct action container
			Actions[currentState].ExecuteAction(clock.PreviousTime, clock.CurrentTime);
		}

		/// <summary>
		/// Check if a state is an attack state
		/// </summary>
		/// <param name="state">The state to check if is an attack state</param>
		/// <returns>bool: whether or not the requested state has an attack</returns>
		public bool IsStateAttack(int state)
		{
			return Actions[state].IsAttack;
		}

		/// <summary>
		/// Check if an attack is active in this state.  Used to queue moves during a combo.
		/// </summary>
		/// <returns>bool: this state hasnt yet entered the recovery phase.  
		/// The move should be queued until it finishes.</returns>
		public bool IsAttackActive(GameClock clock, int currentState)
		{
			//check if the current state is an attack state, and if an attack is active
			return Actions[currentState].IsAttackActive(clock);
		}

		/// <summary>
		/// Replace all the base object pointers in this dude to point to a replacement object
		/// </summary>
		/// <param name="bot">the replacement dude</param>
		public void ReplaceOwner(BaseObject bot)
		{
			//replace in all the state actions
			for (int i = 0; i < Actions.Count; i++)
			{
				Actions[i].ReplaceOwner(bot);
			}
		}

		public StateActions GetStateActions(int stateIndex)
		{
			return Actions[stateIndex];
		}

		#endregion //Methods
	}
}