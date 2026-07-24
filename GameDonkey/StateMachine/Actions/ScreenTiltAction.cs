using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class ScreenTiltAction : BaseAction
    {
        #region Properties

        public float Angle { get; set; }
        public float Duration { get; set; }
        public float Delta { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public ScreenTiltAction(BaseObject owner) :
            base(owner, EActionType.ScreenTilt)
        {
            Angle = 3.0f;
            Duration = 0.4f;
            Delta = 0.2f;
        }

        public ScreenTiltAction(BaseObject owner, ScreenTiltActionModel actionModel) :
            base(owner, actionModel)
        {
            Angle = actionModel.Angle;
            Duration = actionModel.Duration;
            Delta = actionModel.Delta;
        }

        public ScreenTiltAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as ScreenTiltActionModel)
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
            ScreenFX.TriggerScreenTilt(Angle, Duration, Delta);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
