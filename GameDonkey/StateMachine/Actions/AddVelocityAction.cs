using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class AddVelocityAction : BaseAction
	{
		#region Properties

		/// <summary>
		/// The direction to add to the players velocity to when this action is run.
		/// </summary>
		public ActionDirection Velocity { get; set; }

		#endregion //Properties

		#region Initialization

		public AddVelocityAction(BaseObject owner) :
			base(owner, EActionType.AddVelocity)
		{
			Velocity = new ActionDirection();
		}

		public AddVelocityAction(BaseObject owner, AddVelocityActionModel actionModel) :
			base(owner, actionModel)
		{
			Velocity = new ActionDirection(actionModel.Direction);
		}

		public AddVelocityAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as AddVelocityActionModel)
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
			//the final velocity we will add to the character
			Vector2 myVelocity = Velocity.GetDirection(Owner);
			Owner.Velocity += myVelocity;

			return base.Execute();
		}

		#endregion //Methods
	}
}