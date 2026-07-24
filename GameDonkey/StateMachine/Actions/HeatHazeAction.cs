using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class HeatHazeAction : BaseAction
    {
        #region Properties

        public float Strength { get; set; }
        public float Radius { get; set; }
        public float Height { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public HeatHazeAction(BaseObject owner) :
            base(owner, EActionType.HeatHaze)
        {
            Strength = 0.02f;
            Radius = 0.15f;
            Height = 0.40f;
            Duration = 3.0f;
        }

        public HeatHazeAction(BaseObject owner, HeatHazeActionModel actionModel) :
            base(owner, actionModel)
        {
            Strength = actionModel.Strength;
            Radius = actionModel.Radius;
            Height = actionModel.Height;
            Duration = actionModel.Duration;
        }

        public HeatHazeAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as HeatHazeActionModel)
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
            ScreenFX.TriggerHeatHaze(Owner.Position, Strength, Radius, Height, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
