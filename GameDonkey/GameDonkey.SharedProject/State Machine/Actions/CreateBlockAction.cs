using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace GameDonkeyLib
{
	/// <summary>
	/// This action makes a character temporarily invulnerable
	/// </summary>
	public class CreateBlockAction : TimedAction
	{
		#region Properties

		/// <summary>
		/// A list of actions that will be run if this action blocks an attack (sound effects, particle effects, etc)
		/// </summary>
		public List<BaseAction> SuccessActions { get; private set; }

		#endregion //Properties

		#region Initialization

		public CreateBlockAction(BaseObject owner, EActionType actionType = EActionType.CreateBlock) :
			base(owner, actionType)
		{
			SuccessActions = new List<BaseAction>();
		}

		public CreateBlockAction(BaseObject owner, CreateBlockActionModel actionModel) :
			base(owner, actionModel, actionModel.TimeDelta)
		{
			SuccessActions = new List<BaseAction>();
			for (int i = 0; i < actionModel.SuccessActions.Count; i++)
			{
				var stateAction = StateActionFactory.CreateStateAction(actionModel.SuccessActions[i], owner);
				SuccessActions.Add(stateAction);
			}
		}

		public CreateBlockAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as CreateBlockActionModel)
		{
		}

		public override void LoadContent(IGameDonkey engine, SingleStateContainer stateContainer, ContentManager content)
		{
			for (int i = 0; i < SuccessActions.Count; i++)
			{
				SuccessActions[i].LoadContent(engine, stateContainer, content);
			}
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
			for (int i = 0; i < SuccessActions.Count; i++)
			{
				SuccessActions[i].AlreadyRun = false;
			}

			return base.Execute();
		}

		protected virtual void AddBlock()
		{
			Owner.AddBlock(this);
		}

		/// <summary>
		/// execute all the success actions after this attack lands
		/// </summary>
		/// <returns>bool: whether or not a state change occurred while this dude was running</returns>
		public bool ExecuteSuccessActions()
		{
			var result = false;
			for (int i = 0; i < SuccessActions.Count; i++)
			{
				if (SuccessActions[i].Execute())
				{
					result = true;
				}
			}

			return result;
		}

		#endregion //Methods
	}
}