using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class ChromaticAberrationAction : BaseAction
    {
        #region Properties

        public float Distance { get; set; }
        public float Duration { get; set; }
        public FadeCurve Curve { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public ChromaticAberrationAction(BaseObject owner) :
            base(owner, EActionType.ChromaticAberration)
        {
            Distance = 0.1f;
            Duration = 2f;
            Curve = FadeCurve.Linear;
        }

        public ChromaticAberrationAction(BaseObject owner, ChromaticAberrationActionModel actionModel) :
            base(owner, actionModel)
        {
            Distance = actionModel.Distance;
            Duration = actionModel.Duration;
            Curve = actionModel.Curve;
        }

        public ChromaticAberrationAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as ChromaticAberrationActionModel)
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
            ScreenFX.TriggerChromaticAberration(Owner.Position, Distance, Duration, Curve);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
