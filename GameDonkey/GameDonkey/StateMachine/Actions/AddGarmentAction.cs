using AnimationLib;
using FilenameBuddy;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	/// <summary>
	/// This action adds a garment for either a set amount of time, or when the state ends.
	/// </summary>
	public class AddGarmentAction : TimedAction
	{
		#region Properties

		/// <summary>
		/// Reference to the garment to add.
		/// These are loaded from the base object's garment manager
		/// </summary>
		public Garment Garment { get; private set; }

		public Filename Filename { get; set; }

		#endregion //Properties

		#region Initialization

		public AddGarmentAction(BaseObject owner) :
			base(owner, EActionType.AddGarment)
		{
			Filename = new Filename();
		}

		public AddGarmentAction(BaseObject owner, AddGarmentActionModel actionModel) :
			base(owner, actionModel, actionModel.TimeDelta)
		{
			Filename = new Filename(actionModel.Filename);
		}

		public AddGarmentAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as AddGarmentActionModel)
		{
		}

		public override void LoadContent(IGameDonkey engine, ContentManager content)
		{
			//load the garment from the garment manager
			Garment = Owner.Garments.LoadGarment(Filename, engine.Renderer, content);
		}

		#endregion //Initialization

		#region Methods

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			//add this actionto the list of garments
			Owner.Garments.AddAction(this, Owner.CharacterClock);

			return base.Execute();
		}

		#endregion //Methods
	}
}