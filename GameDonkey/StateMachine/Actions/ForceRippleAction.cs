using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class ForceRippleAction : BaseAction
    {
        #region Properties

        public float Strength { get; set; }
        public float Speed { get; set; }
        public float Size { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public ForceRippleAction(BaseObject owner) :
            base(owner, EActionType.ForceRipple)
        {
            Strength = 0.05f;
            Speed = 0.4f;
            Size = 0.08f;
            Duration = 2f;
        }

        public ForceRippleAction(BaseObject owner, ForceRippleActionModel actionModel) :
            base(owner, actionModel)
        {
            Strength = actionModel.Strength;
            Speed = actionModel.Speed;
            Size = actionModel.Size;
            Duration = actionModel.Duration;
        }

        public ForceRippleAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as ForceRippleActionModel)
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
            ScreenFX.TriggerForceRipple(Owner.Position, Strength, Speed, Size, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
