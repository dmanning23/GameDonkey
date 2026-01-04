using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class SendToBackAction : BaseAction
	{
		#region Initialization

		public SendToBackAction(BaseObject owner) :
			base(owner, EActionType.SendToBack)
		{
		}

		public SendToBackAction(BaseObject owner, SendToBackActionModel actionModel) :
			base(owner, actionModel)
		{
		}

		public SendToBackAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as SendToBackActionModel)
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
			Owner.PlayerQueue.SendToBack(Owner);

			//never set these actions to already run
			AlreadyRun = true;
			return true;
		}

		#endregion //Methods
	}
}