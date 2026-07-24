using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class GravityWaveAction : BaseAction
    {
        #region Properties

        public float Strength { get; set; }
        public float StartHeight { get; set; }
        public float EndHeight { get; set; }
        public float Speed { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public GravityWaveAction(BaseObject owner) :
            base(owner, EActionType.GravityWave)
        {
            Strength = 0.04f;
            StartHeight = 0.05f;
            EndHeight = 0.25f;
            Speed = 0.5f;
            Duration = 1.5f;
        }

        public GravityWaveAction(BaseObject owner, GravityWaveActionModel actionModel) :
            base(owner, actionModel)
        {
            Strength = actionModel.Strength;
            StartHeight = actionModel.StartHeight;
            EndHeight = actionModel.EndHeight;
            Speed = actionModel.Speed;
            Duration = actionModel.Duration;
        }

        public GravityWaveAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as GravityWaveActionModel)
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
            ScreenFX.TriggerGravityWave(Owner.Position, Strength, StartHeight, EndHeight, Speed, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
