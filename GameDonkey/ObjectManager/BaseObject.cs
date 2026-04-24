using AnimationLib;
using CameraBuddy;
using DrawListBuddy;
using FilenameBuddy;
using GameTimer;
using HadoukInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ParticleBuddy;
using RenderBuddy;
using StateMachineBuddy;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameDonkeyLib
{
    public abstract class BaseObject
    {
        #region Fields

        private static uint _idCounter;

        // tracks whether an attack landed this state, used by combo engine
        protected bool _attackLanded;

        protected Queue<string> _queuedInput;

        #endregion //Fields

        #region Properties

        #region Required Data Structures

        public uint Id { get; private set; }

        public int QueueId { get; private set; }

        public string ObjectType { get; private set; }

        public AnimationContainer AnimationContainer { get; private set; }

        public IStateContainer States { get; set; }

        public PlayerQueue PlayerQueue { get; set; }

        public BasePhysicsContainer Physics { get; set; }

        protected DrawList DrawList { get; set; }

        public GarmentManager Garments { get; protected set; }

        public string Name { get; set; }

        #endregion //Required Data Structures

        #region State Data

        public HitPauseClock CharacterClock { get; protected set; }

        public TimedActionList<CreateAttackAction> CurrentAttacks { get; set; }

        public TimedActionList<BlockAction> CurrentBlocks { get; set; }

        // when running, attacks don't hit
        public TimedActionList<ShieldAction> ShieldActions { get; set; }

        // when running, no push collisions
        public CountdownTimer EvasionTimer { get; protected set; }

        public CreateThrowAction CurrentThrow { get; set; }

        public CountdownTimer TrailTimer { get; private set; }

        protected TrailAction _trailAction;
        public TrailAction TrailAction
        {
            get { return _trailAction; }
            set
            {
                _trailAction = value;
                if (null != _trailAction)
                {
                    TrailTimer.Start(_trailAction.SpawnDelta);
                }
            }
        }

        public ConstantAccelerationAction AccelAction { get; set; }

        public ConstantDeccelerationAction DeccelAction { get; set; }

        public PlayerQueue LastAttacker { get; protected set; }

        // killed on state change
        public List<Emitter> Emitters { get; private set; }

        // killed on state change
        public List<FlarePointLight> Lights { get; private set; }

        #endregion //State Data

        #region Positional Data

        protected float _height;
        public float Height
        {
            get { return (_height * _scale); }
        }

        protected float _scale;
        public float Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                AnimationContainer.Scale = _scale;
            }
        }

        private Color _playerColor;
        public Color PlayerColor
        {
            get
            {
                return _playerColor;
            }
            set
            {
                _playerColor = value;
                AnimationContainer.Skeleton.RootBone.SetPrimaryColor(_playerColor);
            }
        }

        protected Vector2 _position;
        public virtual Vector2 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }

        public bool Flip { get; set; }

        protected Vector2 _velocity;
        public Vector2 Velocity
        {
            get
            {
                return _velocity;
            }
            set
            {
                _velocity = value;
            }
        }

        // radians/second
        public float RotationPerSecond { get; set; }

        private float _currentRotation = 0.0f;
        public float CurrentRotation
        {
            get
            {
                return _currentRotation;
            }
            set
            {
                _currentRotation = Helper.ClampAngle(value);
            }
        }

        public float Rotation()
        {
            return CurrentRotation;
        }

        public virtual bool Targettable => true;

        #endregion //Positional Data

        #endregion //Properties

        #region Methods

        static BaseObject()
        {
            _idCounter = 0;
        }

        public BaseObject(GameObjectType gameObjectType, HitPauseClock clock, int queueId, string name)
        {
            ObjectType = gameObjectType.ToString();
            Id = BaseObject._idCounter++;
            QueueId = queueId;
            CurrentAttacks = new TimedActionList<CreateAttackAction>();
            CurrentBlocks = new TimedActionList<BlockAction>();
            ShieldActions = new TimedActionList<ShieldAction>();
            EvasionTimer = new CountdownTimer();
            CurrentThrow = null;
            AnimationContainer = new AnimationContainer();
            States = null;
            Position = new Vector2(0.0f);
            Flip = false;
            Velocity = new Vector2(0.0f);
            TrailTimer = new CountdownTimer();
            TrailAction = null;
            PlayerQueue = null;
            _playerColor = Color.White;
            _attackLanded = false;
            _queuedInput = new Queue<string>();
            _height = 0.0f;
            RotationPerSecond = 0.0f;
            CurrentRotation = 0.0f;
            Name = name;

            DrawList = new DrawList();
            Scale = 1f;

            AccelAction = null;
            DeccelAction = null;

            CharacterClock = clock ?? throw new ArgumentNullException("clock");

            LastAttacker = null;

            Garments = new GarmentManager(this);
            Emitters = new List<Emitter>();
            Lights = new List<FlarePointLight>();

            Init();
        }

        // replaces a network player on disconnect, copying their state
        public BaseObject(GameObjectType gamGameObjectType, BaseObject human)
        {
            ObjectType = gamGameObjectType.ToString();
            Id = human.Id;
            Name = human.Name;
            QueueId = human.QueueId;
            CurrentAttacks = human.CurrentAttacks;
            CurrentBlocks = human.CurrentBlocks;
            ShieldActions = human.ShieldActions;
            EvasionTimer = human.EvasionTimer;
            CurrentThrow = human.CurrentThrow;
            AnimationContainer = human.AnimationContainer;
            if (null != States)
            {
                States.StateChangedEvent -= this.StateChanged;
            }
            States = human.States;
            States.StateChangedEvent += this.StateChanged;
            Position = human.Position;
            Flip = human.Flip;
            Velocity = human.Velocity;
            TrailTimer = human.TrailTimer;
            TrailAction = human.TrailAction;
            PlayerQueue = human.PlayerQueue;
            PlayerColor = human.PlayerColor;
            Physics = human.Physics;
            _attackLanded = human._attackLanded;
            _queuedInput = human._queuedInput;
            _height = human._height;
            _scale = human._scale;
            DrawList = human.DrawList;
            AccelAction = human.AccelAction;
            DeccelAction = human.DeccelAction;
            CharacterClock = human.CharacterClock;
            LastAttacker = human.LastAttacker;
            Garments = human.Garments;
        }

        protected virtual void Init()
        {
            Physics = new PlayerPhysicsContainer(this);
            States = new StateContainer();
            States.StateChangedEvent += this.StateChanged;
        }

        public virtual void ReplaceOwner(PlayerObject myBot)
        {
            //should only be called in the child classes!
            Debug.Assert(false);
        }

        public virtual void Reset()
        {
            CurrentAttacks.Reset();
            CurrentBlocks.Reset();
            ShieldActions.Reset();
            EvasionTimer.Stop();
            CurrentThrow = null;
            States.Reset();
            Velocity = Vector2.Zero;
            TrailTimer.Stop();
            TrailAction = null;
            Physics.Reset();
            _attackLanded = false;
            _queuedInput.Clear();
            AccelAction = null;
            DeccelAction = null;
            LastAttacker = null;
            Garments.Reset();
            RotationPerSecond = 0.0f;
            CurrentRotation = 0.0f;

            foreach (var emitter in Emitters)
            {
                emitter.EmitterTimer.Stop();
            }
            Emitters.Clear();

            foreach (var light in Lights)
            {
                light.Kill();
            }
            Lights.Clear();
        }

        public virtual void Update()
        {
            EvasionTimer.Update(CharacterClock);
            TrailTimer.Update(CharacterClock);

            Garments.Update(CharacterClock);

            States.ExecuteActions(CharacterClock);

            UpdateEmitters();

            UpdateAnimation();
        }

        public virtual void UpdateAnimation()
        {
            AnimationContainer.Update(CharacterClock, Position, Flip, CurrentRotation, false);
        }

        protected void UpdateEmitters()
        {
            int i = 0;
            while (i < Emitters.Count)
            {
                var curEmitter = Emitters[i];
                if (curEmitter.IsDead())
                {
                    Emitters.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        protected void UpdateLights()
        {
            int i = 0;
            while (i < Lights.Count)
            {
                var light = Lights[i];
                if (light.IsDead)
                {
                    Lights.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        public virtual void GetPlayerInput(InputWrapper controller, List<IPlayerQueue> listBadGuys, bool ignoreAttackInput)
        {
        }

        public virtual void UpdateInput(InputWrapper controller, IInputState input)
        {
        }

        public virtual void CheckHardCodedStates()
        {
            //TODO: move all this hardcode states junk into update

            Accelerate();

            Deccelerate();

            ApplyRotation();
        }

        public void UpdateRagdoll()
        {
            AnimationContainer.UpdateRagdoll();
        }

        public void AddAttack(CreateAttackAction attackAction)
        {
            CurrentAttacks.AddAction(attackAction, CharacterClock);
        }

        public bool SendStateMessage(string message)
        {
            return States.SendStateMessage(message);
        }

        public void ForceStateChange(string state)
        {
            States.ForceStateChange(state);
        }

        protected virtual void StateChanged(object sender, StateChangeEventArgs<string> eventArgs)
        {
            if (States.CurrentState == "TurningAround" || States.CurrentState == "AirTurningAround")
            {
                Flip = !Flip;
            }

            CurrentAttacks.Reset();

            CurrentBlocks.Reset();
            ShieldActions.Reset();

            EvasionTimer.Stop();

            TrailTimer.Stop();
            TrailAction = null;

            AccelAction = null;
            DeccelAction = null;
            RotationPerSecond = 0.0f;

            Garments.Reset();

            //make sure to update this dude, 
            //because projectiles are activated in the player's update loop and placed in front of them in the update loop
            if (this is ProjectileObject)
            {
                AnimationContainer.Update(CharacterClock, Position, Flip, CurrentRotation, false);
            }
            _attackLanded = false;

            foreach (var emitter in Emitters)
            {
                emitter.EmitterTimer.Stop();
            }
            Emitters.Clear();

            foreach (var light in Lights)
            {
                light.Kill();
            }
            Lights.Clear();
        }

        public virtual void CheckCollisions(BaseObject badGuy)
        {
            if (Id != badGuy.Id)
            {
                Physics.CheckCollisions(badGuy.Physics);
            }
        }

        public virtual void CheckWorldCollisions(Rectangle worldBoundaries)
        {
            Physics.CheckWorldCollisions(Velocity, worldBoundaries);
        }

        #region Collision Responses

        public virtual void CollisionResponse(BasePhysicsContainer otherObject,
            CreateAttackAction attackAction,
            Vector2 firstCollisionPoint,
            Vector2 secondCollisionPoint)
        {
            var player = AttackLanded();

            if (!otherObject.Hits[(int)HitType.Attack].Active || (attackAction.Damage > otherObject.Hits[(int)HitType.Attack].Strength))
            {

                var direction = attackAction.Direction;
                if (Flip)
                {
                    direction.X *= -1.0f;
                }

                otherObject.Hits[(int)HitType.Attack].Set(direction, attackAction, attackAction.Damage, HitType.Attack, this, firstCollisionPoint);

                //perform all the success actions
                if (!otherObject.Owner.IsShielded() && attackAction.ExecuteSuccessActions(otherObject.Owner))
                {
                    //if a state change occurred while the success actions were running, the attack list will be empty
                    CurrentAttacks.Reset();
                }
            }
        }

        public virtual void WeaponCollisionResponse(BasePhysicsContainer otherObject,
            CreateAttackAction attackAction,
            Vector2 firstCollisionPoint,
            Vector2 secondCollisionPoint)
        {
            var rPlayer = AttackLanded();

            //my weapon just collided with that other dude's weapon

            var direction = attackAction.Direction;
            if (Flip)
            {
                direction.X *= -1.0f;
            }

            otherObject.Hits[(int)HitType.Weapon].Set(direction, attackAction, attackAction.Damage, HitType.Weapon, rPlayer, firstCollisionPoint);
        }

        public virtual void BlockResponse(BasePhysicsContainer otherObject,
            CreateAttackAction attackAction,
            BlockAction otherDudesAction,
            Vector2 firstCollisionPoint,
            Vector2 secondCollisionPoint)
        {
            var player = AttackLanded();

            if (!otherObject.Hits[(int)HitType.Block].Active || (attackAction.Damage > otherObject.Hits[(int)HitType.Block].Strength))
            {

                var direction = attackAction.Direction;
                if (Flip)
                {
                    direction.X *= -1.0f;
                }

                otherObject.Hits[(int)HitType.Block].Set(direction, attackAction, attackAction.Damage, HitType.Attack, player, firstCollisionPoint);

                //perform all the success actions for the BLOCKING action not the ATTACKING action!
                otherDudesAction.ExecuteSuccessActions();
            }
        }

        #endregion //Collision Responses

        public bool RemoveAttack(int attackIndex, bool forceRemove = false)
        {
            if (attackIndex < CurrentAttacks.CurrentActions.Count)
            {
                //Only remove attacks if they are not AoE, otherwise they should be able to hit multiple enemies.
                if (!CurrentAttacks.CurrentActions[attackIndex].AoE || forceRemove)
                {
                    CurrentAttacks.CurrentActions.RemoveAt(attackIndex);
                    return true;
                }
            }

            return false;
        }

        #region Hit Response

        public virtual void HitResponse(IGameDonkey engine)
        {
            if (Physics.Hits[(int)HitType.Ground].Active)
            {
                RespondToGroundHit(Physics.Hits[(int)HitType.Ground], engine);
            }
            else if (Physics.Hits[(int)HitType.Ceiling].Active)
            {
                RespondToCeilingHit(Physics.Hits[(int)HitType.Ceiling], engine);
            }

            if (Physics.Hits[(int)HitType.LeftWall].Active)
            {
                RespondToLeftWallHit(Physics.Hits[(int)HitType.LeftWall], engine);
            }
            else if (Physics.Hits[(int)HitType.RightWall].Active)
            {
                RespondToRightWallHit(Physics.Hits[(int)HitType.RightWall], engine);
            }
            CurrentAttacks.Update(CharacterClock);
            CurrentBlocks.Update(CharacterClock);
            ShieldActions.Update(CharacterClock);

            if (null != CurrentThrow)
            {
                //okay, being thrown so don't add velocity
                Position = CurrentThrow.AttackBone.Position;
                Flip = !CurrentThrow.Owner.Flip;
            }
            else
            {
                //no throw, just add the velocity to the position
                Position += Velocity * CharacterClock.TimeDelta;
            }

            Physics.Reset();
        }

        protected virtual void RespondToGroundHit(Hit groundHit, IGameDonkey engine)
        {
            //TODO: override this in projectile and kill the projectile when it hits a wall

            //TOOD: override in level object and do nothing

            //move the player UP out of the floor
            _position.Y += (groundHit.Strength * groundHit.Direction.Y);

            //if the player's velocity is +y, it is set to 0
            if (0f < Velocity.Y)
            {
                _velocity.Y = 0f;
            }
        }

        protected virtual void RespondToCeilingHit(Hit groundHit, IGameDonkey engine)
        {
            //TODO: override this in projectile and kill the projectile when it hits a wall

            //TOOD: override in level object and do nothing

            //move the player down out of the ceiling
            _position.Y += (groundHit.Strength * groundHit.Direction.Y);

            //if the player's velocity is -y, it is set to 0
            if (0f > Velocity.Y)
            {
                _velocity.Y = 0f;
            }
        }

        protected virtual void RespondToLeftWallHit(Hit groundHit, IGameDonkey engine)
        {
            //TODO: override this in projectile and kill the projectile when it hits a wall

            //TOOD: override in level object and do nothing

            //move the player right out of the wall
            _position.X += (groundHit.Strength * groundHit.Direction.X);

            //if the player's velocity is -X, it is set to 0
            if (Velocity.X < 0f)
            {
                _velocity.X = 0f;
            }
        }

        protected virtual void RespondToRightWallHit(Hit groundHit, IGameDonkey engine)
        {
            //TODO: override this in projectile and kill the projectile when it hits a wall

            //TOOD: override in level object and do nothing

            //move the player left out of the wall
            _position.X += (groundHit.Strength * groundHit.Direction.X);

            //if the player's velocity is +X, it is set to 0
            if (0f < Velocity.X)
            {
                _velocity.X = 0f;
            }
        }

        #endregion //Hit Response

        public virtual void AddToCamera(ICamera camera)
        {
            //get half the height
            var halfHeight = (int)(_height * 0.68f);

            //add left/right points
            camera.AddPoint(new Vector2(Position.X - halfHeight, Position.Y));
            camera.AddPoint(new Vector2(Position.X + halfHeight, Position.Y));

            //add the bottom point
            camera.AddPoint(new Vector2(Position.X, Position.Y + (int)(_height * 0.65f)));

            //add the top
            camera.AddPoint(new Vector2(Position.X, Position.Y - (int)(_height * 0.77f)));
        }

        public virtual bool IsShielded()
        {
            return ShieldActions.CurrentActions.Count > 0;
        }

        // sets _attackLanded for combo engine tracking
        public virtual BaseObject AttackLanded()
        {
            _attackLanded = true;
            return this;
        }

        #region Rendering

        public bool DoesNeedCharacterTrail()
        {
            //if there is no trail object, we definitly don't need this
            if (null != TrailAction)
            {
                //check if the trail is still active
                if (CharacterClock.CurrentTime <= TrailAction.DoneTime)
                {
                    //check if the trail timer has expired
                    if (TrailTimer.RemainingTime <= 0.0f)
                    {
                        //eureka, we need a new trail!
                        TrailTimer.Start(TrailAction.SpawnDelta);
                        return true;
                    }
                }
                else
                {
                    //if the trail is expired, set the pointer to 0 to save a cycle next time around
                    TrailAction = null;
                }
            }

            return false;
        }

        public virtual void UpdateDrawlist()
        {
            DrawList.Flush();
            AnimationContainer.Render(DrawList);
        }

        public virtual void Render(IRenderer renderer)
        {
            DrawList.Render(renderer);
        }

        public virtual void RenderCharacterShadow(IGameDonkey engine)
        {
        }

        public void RenderAttacks(IRenderer renderer)
        {
            for (var i = 0; i < CurrentAttacks.CurrentActions.Count; i++)
            {
                if (null != CurrentAttacks.CurrentActions[i].GetCircle())
                {
                    CurrentAttacks.CurrentActions[i].GetCircle().Render(renderer, Color.Red);
                }
            }

            for (var i = 0; i < CurrentBlocks.CurrentActions.Count; i++)
            {
                if (null != CurrentBlocks.CurrentActions[i].GetCircle())
                {
                    CurrentBlocks.CurrentActions[i].GetCircle().Render(renderer, Color.Green);
                }
            }
        }

        public void RenderPhysics(IRenderer renderer)
        {
            AnimationContainer.Skeleton.RootBone.DrawPhysics(renderer, true, Color.White);
        }

        public void DrawCameraInfo(IRenderer renderer)
        {
            //get half the height
            var halfHeight = (int)(_height / 2.0f);

            //add left/right points
            renderer.Primitive.Point(new Vector2(Position.X - halfHeight, Position.Y), Color.Red);
            renderer.Primitive.Point(new Vector2(Position.X + halfHeight, Position.Y), Color.Red);

            //add the bottom point
            renderer.Primitive.Point(new Vector2(Position.X, Position.Y + (int)(_height * 0.55f)), Color.Red);

            //add the top
            renderer.Primitive.Point(new Vector2(Position.X, Position.Y - (int)(_height * 0.8f)), Color.Red);
        }

        #endregion //Rendering

        protected virtual void Accelerate()
        {
            //Is this character acclerating?
            if (null == AccelAction)
            {
                return;
            }

            //Get teh acceleration
            var acceleration = (AccelAction.GetVelocity() * CharacterClock.TimeDelta);

            //Add the acceleration to the velocity
            Velocity += acceleration;

            //Are we going too fast?
            if (Velocity.LengthSquared() > (AccelAction.MaxVelocity * AccelAction.MaxVelocity))
            {
                //Find the amount to pull the velocity back... 

                //Get the length of the acceleration
                var accelLength = acceleration.Length();

                //Get the delta of how much speed we need to shed
                var velocityDif = Velocity.Length() - AccelAction.MaxVelocity;

                //If it is less than the amount of accleration added, use the delta
                if (accelLength > velocityDif)
                {
                    velocityDif = accelLength;
                }

                //Get the opposite direction from the accleration
                var oppositeDir = Velocity * -1.0f;
                oppositeDir.Normalize();

                //Multiply speed delta by the unit vector of the opposite direction
                var decel = velocityDif * oppositeDir;

                //add to the velocity
                Velocity += decel;
            }
        }

        protected void Deccelerate()
        {
            //Is this character decclerating?
            if (null == DeccelAction)
            {
                return;
            }

            //Get teh acceleration
            var decceleration = (DeccelAction.GetVelocity() * CharacterClock.TimeDelta);

            //set the y velocity
            if (Velocity.Y <= DeccelAction.MinYVelocity)
            {
                decceleration.Y = Velocity.Y + Math.Abs(decceleration.Y);
                _velocity.Y = MathHelper.Clamp(decceleration.Y, Velocity.Y, DeccelAction.MinYVelocity);
            }
            else
            {
                decceleration.Y = Velocity.Y - Math.Abs(decceleration.Y);
                _velocity.Y = MathHelper.Clamp(decceleration.Y, DeccelAction.MinYVelocity, Velocity.Y);
            }

            //set the X velocity
            if (Velocity.X <= 0.0f)
            {
                decceleration.X = Velocity.X + Math.Abs(decceleration.X);
                _velocity.X = MathHelper.Clamp(decceleration.X, Velocity.X, 0.0f);
            }
            else
            {
                decceleration.X = Velocity.X - Math.Abs(decceleration.X);
                _velocity.X = MathHelper.Clamp(decceleration.X, 0.0f, Velocity.X);
            }
        }

        public void ApplyRotation()
        {
            //Is this character rotating?
            if (0.0f == RotationPerSecond)
            {
                return;
            }

            //add the rotation to the current rotation
            if (Flip)
            {
                CurrentRotation -= RotationPerSecond * CharacterClock.TimeDelta;
            }
            else
            {
                CurrentRotation += RotationPerSecond * CharacterClock.TimeDelta;
            }
            CurrentRotation = Helper.ClampAngle(CurrentRotation);
        }

        public float MinDistance()
        {
            if (CurrentAttacks.CurrentActions.Count > 0)
            {
                //get teh distance to the nearest attack
                var minDistance = 0f;
                for (var i = 0; i < CurrentAttacks.CurrentActions.Count; i++)
                {
                    if (null != CurrentAttacks.CurrentActions[i].GetCircle())
                    {
                        //get the distance along the x axis to the edge of the attack
                        var attackDistance = CurrentAttacks.CurrentActions[i].GetCircle().GetXDistance(Position);
                        if ((attackDistance > minDistance) && (attackDistance != 0.0f))
                        {
                            minDistance = attackDistance;
                        }
                    }
                }

                //get the distance to the nearest block
                for (var i = 0; i < CurrentBlocks.CurrentActions.Count; i++)
                {
                    if (null != CurrentAttacks.CurrentActions[i].GetCircle())
                    {
                        //get the distance along the x axis to the edge of the attack
                        var attackDistance = CurrentBlocks.CurrentActions[i].GetCircle().GetXDistance(Position);
                        if ((attackDistance > minDistance) && (attackDistance != 0.0f))
                        {
                            minDistance = attackDistance;
                        }
                    }
                }

                return minDistance;
            }
            else
            {
                //no attacks, return the forward edge of the character
                return Height * 0.17f;
            }
        }

        public float MaxDistance()
        {
            float maxDistance = Height * 0.55f;

            //no attacks, return the forward edge of the character
            return maxDistance;
        }

        virtual public Vector2 Direction()
        {
            return Vector2.Zero;
        }

        public virtual void KillPlayer()
        {
        }

        public override string ToString()
        {
            return Name;
        }

        #region Tools

        public void GetAllWeaponBones(List<string> listWeapons)
        {
            //get all the weapons from this dude's model
            AnimationContainer.Skeleton.RootBone.GetAllWeaponBones(listWeapons);

            //get all the weapons loaded into the garment manager
            Garments.GetAllWeaponBones(listWeapons);
        }

        #endregion //Tools

        #endregion //Methods

        #region File IO

        public virtual void ParseXmlData(BaseObjectModel model, IGameDonkey engine, ContentManager content)
        {
            //read in the model
            Scale = model.Scale;
            AnimationContainer.ReadSkeletonXml(model.Model, engine.Renderer, content);
            Physics.SortBones(AnimationContainer.Skeleton.RootBone);

            //read in the animations
            AnimationContainer.ReadAnimationXml(model.Animations, content);

            //read in the garments
            foreach (var garmentFile in model.Garments)
            {
                //Load up the garment.
                var myGarment = LoadXmlGarment(engine, garmentFile, content);
            }

            //read in the states
            States.LoadContent(model, this, engine, content);

            //read in the height
            _height = model.Height;
        }

        public Garment LoadXmlGarment(IGameDonkey engine, Filename garmentFile, ContentManager content)
        {
            //load the garment
            var myGarment = new Garment(garmentFile, AnimationContainer.Skeleton, engine.Renderer, content);

            //add the garment to the dude
            myGarment.AddToSkeleton();

            //sort all the bones in the physics engine
            Physics.SortBones(AnimationContainer.Skeleton.RootBone);
            Physics.GarmentChange(myGarment);

            return myGarment;
        }

        #endregion //File IO
    }
}