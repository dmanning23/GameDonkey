using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class SetVelocityAction : BaseAction
	{
		#region Properties
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

		public override void LoadContent(IGameDonkey engine, ContentManager content)
		{
		}

		#endregion //Initialization

		#region Methods
		public override bool Execute()
		{
			Owner.Velocity = Velocity.GetDirection(Owner);

			return base.Execute();
		}

		#endregion //Methods
	}
}