using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

namespace GameDonkeyLib
{
	public class RandomAction : BaseAction, IStateActionsList
	{
		#region Properties

		Random _random = new Random();

		/// <summary>
		/// When this action is run, it will choose one random action from this list.
		/// </summary>
		private StateActionsList StateActionsList { get; set; }

		public List<BaseAction> Actions => StateActionsList.Actions;

		#endregion //Properties

		#region Initialization

		public RandomAction(BaseObject owner, EActionType actionType = EActionType.Random) :
			base(owner, actionType)
		{
			StateActionsList = new StateActionsList();
		}

		public RandomAction(BaseObject owner, RandomActionModel actionModel, IStateContainer stateContainer) :
			base(owner, actionModel)
		{
			StateActionsList = new StateActionsList();
			StateActionsList.LoadStateActions(actionModel.ActionModels, owner, stateContainer);
		}

		public RandomAction(BaseObject owner, BaseActionModel actionModel, IStateContainer stateContainer) :
			this(owner, actionModel as RandomActionModel, stateContainer)
		{
		}

		public override void LoadContent(IGameDonkey engine, ContentManager content)
		{
			StateActionsList.LoadContent(engine, content);
		}

		#endregion //Initialization

		#region Methods

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			//reset teh success actions
			for (var i = 0; i < StateActionsList.Actions.Count; i++)
			{
				StateActionsList.Actions[i].AlreadyRun = false;
			}

			//Pick one of the success actions and run it
			var index = _random.Next(0, StateActionsList.Actions.Count);
			StateActionsList.Actions[index].Execute();

			return base.Execute();
		}

		public BaseAction AddNewActionFromType(EActionType actionType, BaseObject owner, IGameDonkey engine, ContentManager content)
		{
			return StateActionsList.AddNewActionFromType(actionType, owner, engine, content);
		}

		public void LoadStateActions(StateActionsListModel actionModels, BaseObject owner, IStateContainer stateContainer)
		{
			StateActionsList.LoadStateActions(actionModels, owner, stateContainer);
		}

		public bool RemoveAction(BaseAction action)
		{
			return StateActionsList.RemoveAction(action);
		}

		public void Sort()
		{
			StateActionsList.Sort();
		}

		#endregion //Methods
	}
}