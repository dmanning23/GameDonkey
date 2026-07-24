using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class SmokeAction : BaseAction
    {
        #region Properties

        public Color SmokeColor { get; set; }
        public float Radius { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public SmokeAction(BaseObject owner) :
            base(owner, EActionType.Smoke)
        {
            SmokeColor = Color.White;
            Radius = 0.15f;
            Duration = 2.0f;
        }

        public SmokeAction(BaseObject owner, SmokeActionModel actionModel) :
            base(owner, actionModel)
        {
            SmokeColor = actionModel.SmokeColor;
            Radius = actionModel.Radius;
            Duration = actionModel.Duration;
        }

        public SmokeAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as SmokeActionModel)
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
            ScreenFX.TriggerSmoke(Owner.Position, SmokeColor, Radius, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
