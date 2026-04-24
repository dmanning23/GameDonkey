using GameTimer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;
using System;
using System.Diagnostics;

namespace GameDonkeyLib
{
    public class ProjectileObject : BaseObject
    {
        #region Members
        BaseObject PlayerOwner;
        public bool WeaponHits { get; private set; }

        #endregion //Members

        #region Methods

        public ProjectileObject(HitPauseClock clock, BaseObject playerOwner, int queueId, string name) : base(GameObjectType.Projectile, clock, queueId, name)
        {
            PlayerOwner = playerOwner;
        }

        protected override void Init()
        {
            Physics = new ProjectilePhysicsContainer(this);
            States = new StateContainer("Projectile Object");
            States.StateChangedEvent += this.StateChanged;
        }

        public override void CheckCollisions(BaseObject badGuy)
        {
            //Don't check for collisions with other projectiles >:)
            if (!(badGuy is ProjectileObject))
            {
                base.CheckCollisions(badGuy);
            }
        }

        protected override void RespondToGroundHit(Hit groundHit, IGameDonkey engine)
        {
            base.RespondToGroundHit(groundHit, engine);
            SetHitWallMessage();
        }

        protected override void RespondToCeilingHit(Hit groundHit, IGameDonkey engine)
        {
            base.RespondToCeilingHit(groundHit, engine);
            SetHitWallMessage();
        }

        protected override void RespondToLeftWallHit(Hit groundHit, IGameDonkey engine)
        {
            base.RespondToLeftWallHit(groundHit, engine);
            SetHitWallMessage();
        }

        protected override void RespondToRightWallHit(Hit groundHit, IGameDonkey engine)
        {
            base.RespondToRightWallHit(groundHit, engine);
            SetHitWallMessage();
        }

        private void SetHitWallMessage()
        {
            if (States.StateMachine.Messages.Contains("HitWall"))
            {
                SendStateMessage("HitWall");
            }
        }
        public override BaseObject AttackLanded()
        {
            _attackLanded = true;
            PlayerOwner.AttackLanded();
            return PlayerOwner;
        }
        public override void ReplaceOwner(PlayerObject bot)
        {
            PlayerOwner = bot;
        }
        public override void ParseXmlData(BaseObjectModel model, IGameDonkey engine, ContentManager content)
        {
            ProjectileObjectModel data = model as ProjectileObjectModel;
            if (null == data)
            {
                throw new Exception("must pass ProjectileObjectModel to ProjectileObject.ParseXmlData");
            }

            WeaponHits = data.Weaponhits;
            base.ParseXmlData(data, engine, content);
        }

        public override void UpdateAnimation()
        {
            CurrentRotation = -1f * Helper.atan2(Velocity);

            if (Flip)
            {
                CurrentRotation += MathHelper.Pi;
            }

            base.UpdateAnimation();
        }

        #endregion //Methods
    }
}