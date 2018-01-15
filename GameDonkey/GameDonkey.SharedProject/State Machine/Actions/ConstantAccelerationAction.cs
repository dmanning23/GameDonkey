using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class ConstantAccelerationAction : BaseAction
	{
		#region Properties

		/// <summary>
		/// The pixels/second to add to this characters velocity every second.
		/// </summary>
		public ActionDirection Velocity { get; set; }

		/// <summary>
		/// The point at which to stop adding velocity to the character
		/// </summary>
		public float MaxVelocity { get; set; }

		#endregion //Properties

		#region Initialization

		public ConstantAccelerationAction(BaseObject owner) :
			base(owner, EActionType.ConstantAcceleration)
		{
			Velocity = new ActionDirection();
		}

		public ConstantAccelerationAction(BaseObject owner, ConstantAccelerationActionModel actionModel) :
			base(owner, actionModel)
		{
			Velocity = new ActionDirection(actionModel.Direction);
			MaxVelocity = actionModel.MaxVelocity;
		}

		public ConstantAccelerationAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as ConstantAccelerationActionModel)
		{
		}

		public override void LoadContent(IGameDonkey engine, IStateContainer stateContainer, ContentManager content)
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
			//set the constant accleration variable in the base object
			Owner.AccelAction = this;

			return base.Execute();
		}

		public Vector2 GetVelocity()
		{
			return Velocity.GetDirection(Owner);
		}

		#endregion //Methods
	}
}