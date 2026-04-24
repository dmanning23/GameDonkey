using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class ConstantDeccelerationAction : BaseAction
	{
		#region Properties
		public ActionDirection Velocity { get; set; }
		public float MinYVelocity { get; set; }

		#endregion //Properties

		#region Initialization

		public ConstantDeccelerationAction(BaseObject owner) :
			base(owner, EActionType.ConstantDecceleration)
		{
			Velocity = new ActionDirection();
		}

		public ConstantDeccelerationAction(BaseObject owner, ConstantDeccelerationActionModel actionModel) :
			base(owner, actionModel)
		{
			Velocity = new ActionDirection(actionModel.Direction);
			MinYVelocity = actionModel.MinYVelocity;
		}

		public ConstantDeccelerationAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as ConstantDeccelerationActionModel)
		{
		}

		public override void LoadContent(IGameDonkey engine, ContentManager content)
		{
		}

		#endregion //Initialization

		#region Methods
		public override bool Execute()
		{
			//set the constant accleration variable in the base object
			Owner.DeccelAction = this;

			return base.Execute();
		}

		public Vector2 GetVelocity()
		{
			return Velocity.GetDirection(Owner);
		}

		#endregion //Methods
	}
}