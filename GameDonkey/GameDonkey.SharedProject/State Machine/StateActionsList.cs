using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Linq;

namespace GameDonkeyLib
{
	/// <summary>
	/// A list of actions
	/// </summary>
	public class StateActionsList : IStateActionsList
	{
		#region Properties

		/// <summary>
		/// List of all the actions to perform when in this state
		/// </summary>
		public List<BaseAction> Actions { get; private set; }

		#endregion //Properties

		#region Methods

		#region Initialization

		/// <summary>
		/// standard constructor!
		/// </summary>
		public StateActionsList()
		{
			Actions = new List<BaseAction>();
		}

		public virtual void LoadStateActions(StateActionsListModel actionModels, BaseObject owner, IStateContainer stateContainer)
		{
			for (int i = 0; i < actionModels.ActionModels.Count; i++)
			{
				var stateAction = StateActionFactory.CreateStateAction(actionModels.ActionModels[i], owner, stateContainer);
				Actions.Add(stateAction);
			}
		}

		public virtual void LoadContent(IGameDonkey engine, ContentManager content)
		{
			for (int i = 0; i < Actions.Count; i++)
			{
				Actions[i].LoadContent(engine, content);
			}

			Sort();
		}

		#endregion //Initialization

		#region Tool Methods

		/// <summary>
		/// Given an action type, add a blank action to this list of actions
		/// </summary>
		/// <param name="actionType">the type of action to add</param>
		/// <param name="owner">the owner of this action list</param>
		/// <returns>IBaseAction: reference to the action that was created</returns>
		public BaseAction AddNewActionFromType(EActionType actionType, BaseObject owner, IGameDonkey engine, ContentManager content)
		{
			//get the correct action type
			var action = StateActionFactory.CreateStateAction(actionType, owner);
			action.LoadContent(engine, content);

			//save the action
			Actions.Add(action);

			//sort the list of actions
			Sort();

			//return the newly created dude
			return action;
		}

		/// <summary>
		/// remove an item from the state actions
		/// </summary>
		/// <param name="iActionIndex">index of the item to remove</param>
		public bool RemoveAction(BaseAction action)
		{
			return Actions.Remove(action);
		}

		public void Sort()
		{
			Actions.Sort(new ActionSort());
		}

		public BaseAction FindAction(string id)
		{
			return Actions.FirstOrDefault(x => x.Id == id);
		}

		#endregion //Tool Methods

		#endregion //Methods
	}
}