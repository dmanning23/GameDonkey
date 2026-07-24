using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class HitFlashAction : BaseAction
    {
        #region Properties

        public Color BlendColor { get; set; }
        public FadeMode Mode { get; set; }
        public FadeCurve Curve { get; set; }
        public EffectBlendMode BlendMode { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public HitFlashAction(BaseObject owner) :
            base(owner, EActionType.HitFlash)
        {
            BlendColor = Color.White;
            Mode = FadeMode.FadeOut;
            Curve = FadeCurve.Linear;
            BlendMode = EffectBlendMode.LinearDodge;
            Duration = 1f;
        }

        public HitFlashAction(BaseObject owner, HitFlashActionModel actionModel) :
            base(owner, actionModel)
        {
            BlendColor = actionModel.BlendColor;
            Mode = actionModel.Mode;
            Curve = actionModel.Curve;
            BlendMode = actionModel.BlendMode;
            Duration = actionModel.Duration;
        }

        public HitFlashAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as HitFlashActionModel)
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
            ScreenFX.TriggerHitFlash(BlendColor, Mode, Curve, BlendMode, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
