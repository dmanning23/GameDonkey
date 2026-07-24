using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class FreezeFrameAction : BaseAction
    {
        #region Properties

        public Color TintColor { get; set; }
        public float FlashIn { get; set; }
        public float Hold { get; set; }
        public float FadeOut { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public FreezeFrameAction(BaseObject owner) :
            base(owner, EActionType.FreezeFrame)
        {
            TintColor = Color.White;
            FlashIn = 0.10f;
            Hold = 0.40f;
            FadeOut = 0.30f;
        }

        public FreezeFrameAction(BaseObject owner, FreezeFrameActionModel actionModel) :
            base(owner, actionModel)
        {
            TintColor = actionModel.TintColor;
            FlashIn = actionModel.FlashIn;
            Hold = actionModel.Hold;
            FadeOut = actionModel.FadeOut;
        }

        public FreezeFrameAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as FreezeFrameActionModel)
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
            ScreenFX.TriggerFreezeFrame(TintColor, FlashIn, Hold, FadeOut);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
