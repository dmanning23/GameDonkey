using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class ConstantDeccelerationAction : BaseAction
	{
		#region Properties

		/// <summary>
		/// The pixels/second to add to this characters velocity every second.
		/// should be -x and +y
		/// </summary>
		public ActionDirection Velocity { get; set; }

		/// <summary>
		/// The point at which to stop adding y velocity to the character
		/// should be +y.  Don't care about x, that will always slow down to 0.0
		/// </summary>
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

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
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