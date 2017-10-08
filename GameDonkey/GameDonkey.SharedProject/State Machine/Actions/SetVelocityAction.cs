using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class SetVelocityAction : BaseAction
	{
		#region Properties

		/// <summary>
		/// The direction to set the players velocity to when this action is run
		/// </summary>
		public ActionDirection Velocity { get; set; }

		#endregion //Properties

		#region Initialization

		public SetVelocityAction(BaseObject owner) :
			base(owner, EActionType.SetVelocity)
		{
			Velocity = new ActionDirection();
		}

		public SetVelocityAction(BaseObject owner, SetVelocityActionModel actionModel) :
			base(owner, actionModel)
		{
			Velocity = new ActionDirection(actionModel.Direction);
		}

		public SetVelocityAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as SetVelocityActionModel)
		{
		}

		public override void LoadContent(IGameDonkey engine, SingleStateContainer stateContainer, ContentManager content)
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
			Owner.Velocity = Velocity.GetDirection(Owner);

			return base.Execute();
		}

		#endregion //Methods
	}
}