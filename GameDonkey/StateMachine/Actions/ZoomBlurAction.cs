using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class ZoomBlurAction : BaseAction
    {
        #region Properties

        public float Strength { get; set; }
        public float Radius { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public ZoomBlurAction(BaseObject owner) :
            base(owner, EActionType.ZoomBlur)
        {
            Strength = 0.05f;
            Radius = 1.0f;
            Duration = 0.4f;
        }

        public ZoomBlurAction(BaseObject owner, ZoomBlurActionModel actionModel) :
            base(owner, actionModel)
        {
            Strength = actionModel.Strength;
            Radius = actionModel.Radius;
            Duration = actionModel.Duration;
        }

        public ZoomBlurAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as ZoomBlurActionModel)
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
            ScreenFX.TriggerZoomBlur(Owner.Position, Strength, Radius, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
