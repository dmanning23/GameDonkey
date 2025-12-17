using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class DeactivateAction : BaseAction
	{
		#region Initialization

		public DeactivateAction(BaseObject owner) :
			base(owner, EActionType.Deactivate)
		{
		}

		public DeactivateAction(BaseObject owner, DeactivateActionModel actionModel) :
			base(owner, actionModel)
		{
		}

		public DeactivateAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as DeactivateActionModel)
		{
		}

		public override void LoadContent(IGameDonkey engine, ContentManager content)
		{
		}

		#endregion //Initialization

		#region Methods

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			//get that dude's characterqeueu and deactiuvate it
			Owner.PlayerQueue.DeactivateObject(Owner);

			//never set these actions to already run
			AlreadyRun = false;
			return true;
		}

		#endregion //Methods
	}
}