using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	/// <summary>
	/// This action makes a character temporarily invulnerable
	/// </summary>
	public class CreateBlockAction : TimedAction
	{
		#region Initialization

		public CreateBlockAction(BaseObject owner) :
			base(owner, EActionType.CreateBlock)
		{
		}

		public CreateBlockAction(BaseObject owner, CreateBlockActionModel actionModel) :
			base(owner, actionModel, actionModel.TimeDelta)
		{
		}

		public CreateBlockAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as CreateBlockActionModel)
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
			Owner.AddBlock(this);

			return base.Execute();
		}

		#endregion //Methods
	}
}