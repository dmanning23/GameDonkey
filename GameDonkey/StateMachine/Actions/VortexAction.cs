using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;

namespace GameDonkeyLib
{
    public class VortexAction : BaseAction
    {
        #region Properties

        public float Radius { get; set; }
        public float Speed { get; set; }
        public float SpinInTime { get; set; }
        public float SpinOutTime { get; set; }
        public FadeCurve VortexFadeCurve { get; set; }
        public bool Clockwise { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public VortexAction(BaseObject owner) :
            base(owner, EActionType.Vortex)
        {
            Radius = 0.25f;
            Speed = 1f;
            SpinInTime = 0.3f;
            SpinOutTime = 0.3f;
            VortexFadeCurve = FadeCurve.Linear;
            Clockwise = true;
        }

        public VortexAction(BaseObject owner, VortexActionModel actionModel) :
            base(owner, actionModel)
        {
            Radius = actionModel.Radius;
            Speed = actionModel.Speed;
            SpinInTime = actionModel.SpinInTime;
            SpinOutTime = actionModel.SpinOutTime;
            VortexFadeCurve = actionModel.VortexFadeCurve;
            Clockwise = actionModel.Clockwise;
        }

        public VortexAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as VortexActionModel)
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
            ScreenFX.TriggerVortex(Owner.Position, Radius, Speed, SpinInTime, SpinOutTime, VortexFadeCurve, Clockwise);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
