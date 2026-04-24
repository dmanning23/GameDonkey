using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class CreateThrowAction : CreateAttackAction
	{
		#region Properties

		//After the throw connects:
		public string ThrowMessage { get; set; }
		public float ReleaseTimeDelta { get; set; }
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