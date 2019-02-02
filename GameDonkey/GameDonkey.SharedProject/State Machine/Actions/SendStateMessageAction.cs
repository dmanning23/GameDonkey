using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class SendStateMessageAction : BaseAction
	{
		#region Properties

		/// <summary>
		/// name of the message this dude sends
		/// </summary>
		public string Message { get; set; }

		private IStateContainer StateContainer { get; set; }

		#endregion //Properties

		#region Initialization

		public SendStateMessageAction(BaseObject owner) :
			base(owner, EActionType.SendStateMessage)
		{
		}

		public SendStateMessageAction(BaseObject owner, SendStateMessageActionModel actionModel) :
			base(owner, actionModel)
		{
			Message = actionModel.Message;
		}

		public SendStateMessageAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as SendStateMessageActionModel)
		{
		}

		public override void LoadContent(IGameDonkey engine, IStateContainer stateContainer, ContentManager content)
		{
			StateContainer = stateContainer;
		}

		#endregion //Initialization

		#region Methods

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			//The message offset is added to this message when it is read in, so dont add anything
			StateContainer.SendStateMessage(Message);

			//keep running the action until it goes through?
			AlreadyRun = true;
			return true;
		}

		#endregion //Methods
	}
}