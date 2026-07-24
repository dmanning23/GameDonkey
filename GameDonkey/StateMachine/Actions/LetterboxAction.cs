using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class LetterboxAction : BaseAction
    {
        #region Properties

        public float BarHeight { get; set; }
        public float SlideIn { get; set; }
        public float Hold { get; set; }
        public float SlideOut { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public LetterboxAction(BaseObject owner) :
            base(owner, EActionType.Letterbox)
        {
            BarHeight = 0.10f;
            SlideIn = 0.15f;
            Hold = 1.00f;
            SlideOut = 0.15f;
        }

        public LetterboxAction(BaseObject owner, LetterboxActionModel actionModel) :
            base(owner, actionModel)
        {
            BarHeight = actionModel.BarHeight;
            SlideIn = actionModel.SlideIn;
            Hold = actionModel.Hold;
            SlideOut = actionModel.SlideOut;
        }

        public LetterboxAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as LetterboxActionModel)
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
            ScreenFX.TriggerLetterbox(BarHeight, SlideIn, Hold, SlideOut);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
