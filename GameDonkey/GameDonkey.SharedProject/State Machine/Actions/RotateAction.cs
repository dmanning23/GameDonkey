using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class RotateAction : BaseAction
	{
		#region Properties

		/// <summary>
		/// The radians/second to rotate
		/// </summary>
		public float Rotation { get; set; }

		#endregion //Properties

		#region Initialization

		public RotateAction(BaseObject owner) :
			base(owner, EActionType.Rotate)
		{
		}

		public RotateAction(BaseObject owner, RotateActionModel actionModel) :
			base(owner, actionModel)
		{
			Rotation = actionModel.Rotation;
		}

		public RotateAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as RotateActionModel)
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
			//set the rotation action variable in the base object
			Owner.RotationPerSecond = Rotation;

			return base.Execute();
		}

		#endregion //Methods
	}
}