using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class GlassShatterAction : BaseAction
    {
        #region Properties

        public float Strength { get; set; }
        public int NumCells { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public GlassShatterAction(BaseObject owner) :
            base(owner, EActionType.GlassShatter)
        {
            Strength = 0.04f;
            NumCells = 20;
            Duration = 0.8f;
        }

        public GlassShatterAction(BaseObject owner, GlassShatterActionModel actionModel) :
            base(owner, actionModel)
        {
            Strength = actionModel.Strength;
            NumCells = actionModel.NumCells;
            Duration = actionModel.Duration;
        }

        public GlassShatterAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as GlassShatterActionModel)
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
            ScreenFX.TriggerGlassShatter(Owner.Position, Strength, NumCells, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
