using AnimationLib;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
    public class PlayAnimationAction : BaseAction
    {
        #region Properties
        public string AnimationName { get; set; }
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
        public override bool Execute(float currentTime)
        {
            Owner.AnimationContainer.SetAnimation(AnimationName, PlaybackMode);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}