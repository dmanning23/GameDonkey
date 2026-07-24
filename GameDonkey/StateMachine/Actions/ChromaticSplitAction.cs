using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class ChromaticSplitAction : BaseAction
    {
        #region Properties

        public float MaxDistance { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public ChromaticSplitAction(BaseObject owner) :
            base(owner, EActionType.ChromaticSplit)
        {
            MaxDistance = 0.05f;
            Duration = 0.3f;
        }

        public ChromaticSplitAction(BaseObject owner, ChromaticSplitActionModel actionModel) :
            base(owner, actionModel)
        {
            MaxDistance = actionModel.MaxDistance;
            Duration = actionModel.Duration;
        }

        public ChromaticSplitAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as ChromaticSplitActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            ScreenFX = engine.ScreenFX;
        }

        #endregion //Initialization

        #region Methods

        public override bool Execute(float currentTime)
        {
            ScreenFX.TriggerChromaticSplit(Owner.Position, MaxDistance, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
