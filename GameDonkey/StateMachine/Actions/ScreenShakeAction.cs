using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class ScreenShakeAction : BaseAction
    {
        #region Properties

        public float Length { get; set; }
        public float Delta { get; set; }
        public float Amount { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public ScreenShakeAction(BaseObject owner) :
            base(owner, EActionType.ScreenShake)
        {
            Length = 1f;
            Delta = 0.1f;
            Amount = 0.1f;
        }

        public ScreenShakeAction(BaseObject owner, ScreenShakeActionModel actionModel) :
            base(owner, actionModel)
        {
            Length = actionModel.Length;
            Delta = actionModel.Delta;
            Amount = actionModel.Amount;
        }

        public ScreenShakeAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as ScreenShakeActionModel)
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
            ScreenFX.TriggerScreenShake(Length, Delta, Amount);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
