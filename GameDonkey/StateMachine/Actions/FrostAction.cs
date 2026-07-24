using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class FrostAction : BaseAction
    {
        #region Properties

        public Color TintColor { get; set; }
        public float Radius { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public FrostAction(BaseObject owner) :
            base(owner, EActionType.Frost)
        {
            TintColor = Color.White;
            Radius = 0.25f;
            Duration = 1.50f;
        }

        public FrostAction(BaseObject owner, FrostActionModel actionModel) :
            base(owner, actionModel)
        {
            TintColor = actionModel.TintColor;
            Radius = actionModel.Radius;
            Duration = actionModel.Duration;
        }

        public FrostAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as FrostActionModel)
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
            ScreenFX.TriggerFrost(Owner.Position, TintColor, Radius, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
