using AnimationLib;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
	public class PlayAnimationAction : BaseAction
	{
		#region Properties

		/// <summary>
		/// Name of the animation to play
		/// </summary>
		public string AnimationName { get; set; }

		/// <summary>
		/// which playback mode to use
		/// </summary>
		public EPlayback PlaybackMode { get; set; }

		#endregion //Properties

		#region Initialization

		public PlayAnimationAction(BaseObject owner) :
			base(owner, EActionType.PlayAnimation)
		{
		}

		public PlayAnimationAction(BaseObject owner, PlayAnimationActionModel actionModel) :
			base(owner, actionModel)
		{
			AnimationName = actionModel.Animation;
			PlaybackMode = actionModel.Playback;
		}

		public PlayAnimationAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as PlayAnimationActionModel)
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
			Owner.AnimationContainer.SetAnimation(AnimationName, PlaybackMode);

			return base.Execute();
		}

		#endregion //Methods
	}
}