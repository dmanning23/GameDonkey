using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
    public class TrailAction : TimedAction
    {
        #region Properties
        public Color StartColor { get; set; }
        public float TrailLifeDelta { get; set; }
        public float SpawnDelta { get; set; }

        #endregion //Properties

        #region Initialization

        public TrailAction(BaseObject owner) :
            base(owner, EActionType.Trail)
        {
            StartColor = Color.White;
        }

        public TrailAction(BaseObject owner, TrailActionModel actionModel) :
            base(owner, actionModel, actionModel.TimeDelta)
        {
            StartColor = actionModel.Color;
            TrailLifeDelta = actionModel.LifeDelta;
            SpawnDelta = actionModel.SpawnDelta;
        }

        public TrailAction(BaseObject owner, BaseActionModel actionModel) :
            this(owner, actionModel as TrailActionModel)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
        }

        #endregion //Initialization

        #region Methods
        public override bool Execute(float currentTime)
        {
            //activate the trail
            SetDoneTime(Owner.CharacterClock);

            //set the base objects character trail to this dude
            Owner.TrailAction = this;

            //start the base objects trail timer
            Owner.TrailTimer.Start(SpawnDelta);

            return base.Execute(currentTime);
        }

        #endregion //Methods
    }
}