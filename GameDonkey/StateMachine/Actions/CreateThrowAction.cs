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
		public string ThrowMessage { get; set; }

		/// <summary>
		/// the time delta after the grab connects to release the other characters
		/// </summary>
		public float ReleaseTimeDelta { get; set; }

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

		public CreateThrowAction(BaseObject owner, CreateThrowActionModel actionModel, IStateContainer container) :
			base(owner, actionModel, container)
		{
			ThrowMessage = actionModel.ThrowMessage;
			ReleaseTimeDelta = actionModel.ReleaseTimeDelta;
		}

		public CreateThrowAction(BaseObject owner, BaseActionModel actionModel, IStateContainer container) :
			this(owner, actionModel as CreateThrowActionModel, container)
		{
		}

		public override void LoadContent(IGameDonkey engine, ContentManager content)
		{
			base.LoadContent(engine, content);
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