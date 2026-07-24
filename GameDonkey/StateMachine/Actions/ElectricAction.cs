using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class ElectricAction : BaseAction
    {
        #region Properties

        public Color ElectricColor { get; set; }
        public float Radius { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public ElectricAction(BaseObject owner) :
            base(owner, EActionType.Electric)
        {
            ElectricColor = Color.White;
            Radius = 0.20f;
            Duration = 0.60f;
        }

        public ElectricAction(BaseObject owner, ElectricActionModel actionModel) :
            base(owner, actionModel)
        {
            ElectricColor = actionModel.ElectricColor;
            Radius = actionModel.Radius;
            Duration = actionModel.Duration;
        }

        public ElectricAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as ElectricActionModel)
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
            ScreenFX.TriggerElectric(Owner.Position, ElectricColor, Radius, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
