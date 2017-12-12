using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class TrailAction : TimedAction
	{
		#region Properties

		/// <summary>
		/// the start color of this trail
		/// </summary>
		public Color StartColor { get; set; }

		/// <summary>
		/// how long each individual trail lasts
		/// </summary>
		public float TrailLifeDelta { get; set; }

		/// <summary>
		/// how often to spawn a new trail
		/// </summary>
		public float SpawnDelta { get; set; }

		#endregion //Properties

		#region Initialization

		public TrailAction(BaseObject owner) :
			base(owner, EActionType.Trail)
		{
			StartColor = Color.White;
		}

		public TrailAction(BaseObject owner, TrailActionModel actionModel) :
			base(owner, actionModel, actionModel.TimeDelta)
		{
			StartColor = actionModel.Color;
			TrailLifeDelta = actionModel.LifeDelta;
			SpawnDelta = actionModel.SpawnDelta;
		}

		public TrailAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as TrailActionModel)
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
			//activate the trail
			SetDoneTime(Owner.CharacterClock);

			//set the base objects character trail to this dude
			Owner.TrailAction = this;

			//start the base objects trail timer
			Owner.TrailTimer.Start(SpawnDelta);

			return base.Execute();
		}

		#endregion //Methods
	}
}