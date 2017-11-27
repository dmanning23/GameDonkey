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
		protected string _animationName;
		public string AnimationName
		{
			get { return _animationName; }
			set
			{
				_animationName = value;
				if (null != Owner)
				{
					AnimationIndex = Owner.AnimationContainer.FindAnimationIndex(_animationName);
				}
			}
		}

		/// <summary>
		/// the index of the animation to play, set at load time
		/// </summary>
		public int AnimationIndex { get; protected set; }

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
			Owner.AnimationContainer.SetAnimation(AnimationIndex, PlaybackMode);

			return base.Execute();
		}

		#endregion //Methods
	}
}