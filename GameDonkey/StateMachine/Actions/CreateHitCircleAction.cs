using AnimationLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
    public class CreateHitCircleAction : CreateAttackAction
    {
        #region Properties
        protected PhysicsCircle HitCircle;

        public float Radius
        {
            get
            {
                return HitCircle.Radius;
            }
            set
            {
                HitCircle.Radius = value;
            }
        }
        public Vector2 StartOffset { get; set; }
        public Vector2 Velocity { get; set; }

        #endregion //Properties

        #region Initialization

        public CreateHitCircleAction(BaseObject owner) :
            base(owner, EActionType.CreateHitCircle)
        {
            HitCircle = new PhysicsCircle();
            StartOffset = Vector2.Zero;
            Velocity = Vector2.Zero;
        }

        public CreateHitCircleAction(BaseObject owner, CreateHitCircleActionModel actionModel, IStateContainer container) :
            base(owner, actionModel, container)
        {
            HitCircle = new PhysicsCircle()
            {
                Radius = actionModel.Radius,
            };
            StartOffset = actionModel.StartOffset;
            Velocity = actionModel.Velocity;
        }

        public CreateHitCircleAction(BaseObject owner, BaseActionModel actionModel, IStateContainer container) :
            this(owner, actionModel as CreateHitCircleActionModel, container)
        {
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            base.LoadContent(engine, content);
        }

        #endregion //Initialization

        #region Methods
        public override bool Execute(float currentTime)
        {
            SetAttackBone();

            //set the circle location

            //get the bone location
            var myLocation = AttackBone.AnchorPosition;

            //get the start offset
            var myOffset = StartOffset;
            if (Owner.Flip)
            {
                myOffset.X *= -1.0f;
            }

            //set the circle location
            HitCircle.Reset(myLocation - myOffset);

            return base.Execute(currentTime);
        }

        public override void Update()
        {
            //add the velocity
            var myPosition = HitCircle.Pos + ((Velocity * Owner.Scale) * Owner.CharacterClock.TimeDelta);

            //update the circle location
            HitCircle.Update(myPosition);
        }

        public override PhysicsCircle GetCircle()
        {
            return HitCircle;
        }

        #endregion //Methods
    }
}