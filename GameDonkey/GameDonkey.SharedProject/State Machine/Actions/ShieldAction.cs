using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace GameDonkeyLib
{
	/// <summary>
	/// This action makes a character temporarily invulnerable
	/// </summary>
	public class ShieldAction : TimedAction, IStateActionsList
	{
		#region Properties

		/// <summary>
		/// A list of actions that will be run if this action blocks an attack (sound effects, particle effects, etc)
		/// </summary>
		private StateActionsList StateActionsList { get; set; }

		public List<BaseAction> Actions => StateActionsList.Actions;

		#endregion //Properties

		#region Initialization

		public ShieldAction(BaseObject owner, EActionType actionType = EActionType.Shield) :
			base(owner, actionType)
		{
			StateActionsList = new StateActionsList();
		}

		public ShieldAction(BaseObject owner, ShieldActionModel actionModel, IStateContainer stateContainer) :
			base(owner, actionModel, actionModel.TimeDelta)
		{
			StateActionsList = new StateActionsList();
			StateActionsList.LoadStateActions(actionModel.ActionModels, owner, stateContainer);
		}

		public ShieldAction(BaseObject owner, BaseActionModel actionModel, IStateContainer stateContainer) :
			this(owner, actionModel as ShieldActionModel, stateContainer)
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
			AddBlock();

			//reset teh success actions
			for (int i = 0; i < StateActionsList.Actions.Count; i++)
			{
				StateActionsList.Actions[i].AlreadyRun = false;
			}

			return base.Execute();
		}

		protected virtual void AddBlock()
		{
			Owner.AddShield(this);
		}

		/// <summary>
		/// execute all the success actions after this attack lands
		/// </summary>
		/// <returns>bool: whether or not a state change occurred while this dude was running</returns>
		public bool ExecuteSuccessActions()
		{
			var result = false;
			for (int i = 0; i < StateActionsList.Actions.Count; i++)
			{
				if (StateActionsList.Actions[i].Execute())
				{
					result = true;
				}
			}

			return result;
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