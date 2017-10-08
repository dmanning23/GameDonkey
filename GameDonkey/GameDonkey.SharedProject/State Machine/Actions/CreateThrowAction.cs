using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class CreateThrowAction : CreateAttackAction
	{
		#region Properties

		//After the throw connects:

		/// <summary>
		/// The message to send to the state machine when this grab connects, to switch to the throw
		/// </summary>
		protected string _throwMessageName;
		protected string ThrowMessageName
		{
			get
			{
				return _throwMessageName;
			}
			set
			{
				_throwMessageName = value;
				ThrowMessage = Owner.States.GetMessageIndexFromText(ThrowMessageName);
			}
		}

		/// <summary>
		/// that message, loaded from the state machine
		/// </summary>
		public int ThrowMessage { get; protected set; }

		/// <summary>
		/// the time delta after the grab connects to release the other characters
		/// </summary>
		protected float ReleaseTimeDelta { get; set; }

		/// <summary>
		/// the time to let go of the character, set at runtime when throw is activated
		/// </summary>
		public float TimeToRelease { get; protected set; }

		#endregion //Properties

		#region Initialization

		public CreateThrowAction(BaseObject owner) :
			base(owner, EActionType.CreateThrow)
		{
		}

		public CreateThrowAction(BaseObject owner, CreateThrowActionModel actionModel) :
			base(owner, actionModel)
		{
			ThrowMessageName = actionModel.ThrowMessage;
			ReleaseTimeDelta = actionModel.ReleaseTimeDelta;
		}

		public CreateThrowAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as CreateThrowActionModel)
		{
		}

		public override void LoadContent(IGameDonkey engine, SingleStateContainer stateContainer, ContentManager content)
		{
			base.LoadContent(engine, stateContainer, content);
		}

		#endregion //Initialization

		#region Methods

		/// <summary>
		/// execute all the success actions after this attack lands
		/// </summary>
		/// <param name="characterHit">The dude that got nailed by this attack</param>
		/// <returns>bool: whether or not a state change occurred while this dude was running</returns>
		public override bool ExecuteSuccessActions(BaseObject characterHit)
		{
			//send the state message
			Owner.SendStateMessage(ThrowMessage);

			//activate the throw
			TimeToRelease = characterHit.CharacterClock.CurrentTime + ReleaseTimeDelta;
			characterHit.CurrentThrow = this;

			return base.ExecuteSuccessActions(characterHit);
		}

		#endregion //Methods
	}
}