using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class SendStateMessageAction : BaseAction
	{
		#region Properties

		/// <summary>
		/// name of the message this dude sends
		/// </summary>
		protected string _messageName;
		public string MessageName
		{
			get
			{
				return _messageName;
			}
			set
			{
				_messageName = value;
				SetMessage();
			}
		}


		/// <summary>
		/// the message this dude sends
		/// </summary>
		public int Message { get; protected set; }

		private IStateContainer _stateContainer { get; set; }
		private IStateContainer StateContainer
		{
			get
			{
				return _stateContainer;
			}
			set
			{
				_stateContainer = value;
				SetMessage();
			}
		}

		#endregion //Properties

		#region Initialization

		public SendStateMessageAction(BaseObject owner) :
			base(owner, EActionType.SendStateMessage)
		{
		}

		public SendStateMessageAction(BaseObject owner, SendStateMessageActionModel actionModel) :
			base(owner, actionModel)
		{
			MessageName = actionModel.Message;
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

		private void SetMessage()
		{
			if (null != StateContainer && !string.IsNullOrEmpty(MessageName))
			{
				Message = StateContainer.GetMessageIndexFromText(MessageName);
			}
		}

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