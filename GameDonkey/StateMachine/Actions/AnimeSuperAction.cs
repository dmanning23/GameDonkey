using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class AnimeSuperAction : BaseAction
    {
        #region Properties

        public Color SuperColor { get; set; }
        public float FlashIn { get; set; }
        public float Hold { get; set; }
        public float FadeOut { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public AnimeSuperAction(BaseObject owner) :
            base(owner, EActionType.AnimeSuper)
        {
            SuperColor = Color.White;
            FlashIn = 0.05f;
            Hold = 0.30f;
            FadeOut = 0.40f;
        }

        public AnimeSuperAction(BaseObject owner, AnimeSuperActionModel actionModel) :
            base(owner, actionModel)
        {
            SuperColor = actionModel.SuperColor;
            FlashIn = actionModel.FlashIn;
            Hold = actionModel.Hold;
            FadeOut = actionModel.FadeOut;
        }

        public AnimeSuperAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as AnimeSuperActionModel)
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
            ScreenFX.TriggerAnimeSuper(SuperColor, FlashIn, Hold, FadeOut);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
