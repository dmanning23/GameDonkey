using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ScreenFXBuddy;
using ScreenFXBuddy.Effects;

namespace GameDonkeyLib
{
    public class SpeedLinesAction : BaseAction
    {
        #region Properties

        public Color SpeedLinesColor { get; set; }
        public SpeedLinesMode LinesMode { get; set; }
        public FadeMode SpeedLinesFadeMode { get; set; }
        public FadeCurve SpeedLinesFadeCurve { get; set; }
        public int LineCount { get; set; }
        public float MaxRadius { get; set; }
        public float Duration { get; set; }

        private IScreenFXService ScreenFX { get; set; }

        #endregion //Properties

        #region Initialization

        public SpeedLinesAction(BaseObject owner) :
            base(owner, EActionType.SpeedLines)
        {
            SpeedLinesColor = Color.White;
            LinesMode = SpeedLinesMode.Expand;
            SpeedLinesFadeMode = FadeMode.FadeOut;
            SpeedLinesFadeCurve = FadeCurve.Logarithmic;
            LineCount = 24;
            MaxRadius = 1.0f;
            Duration = 1f;
        }

        public SpeedLinesAction(BaseObject owner, SpeedLinesActionModel actionModel) :
            base(owner, actionModel)
        {
            SpeedLinesColor = actionModel.SpeedLinesColor;
            LinesMode = actionModel.LinesMode;
            SpeedLinesFadeMode = actionModel.SpeedLinesFadeMode;
            SpeedLinesFadeCurve = actionModel.SpeedLinesFadeCurve;
            LineCount = actionModel.LineCount;
            MaxRadius = actionModel.MaxRadius;
            Duration = actionModel.Duration;
        }

        public SpeedLinesAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as SpeedLinesActionModel)
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
            ScreenFX.TriggerSpeedLines(Owner.Position, SpeedLinesColor, LinesMode, SpeedLinesFadeMode, SpeedLinesFadeCurve, LineCount, MaxRadius, Duration);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}
