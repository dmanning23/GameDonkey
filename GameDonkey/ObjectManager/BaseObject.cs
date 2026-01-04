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
    /// <summary>
    /// this is a game token, either player or projectile
    /// </summary>
    public abstract class BaseObject
    {
        #region Fields

        /// <summary>
        /// this is a counter for assigning round-robin item ids, this is the next id to use
        /// </summary>
        private static uint _idCounter;

        /// <summary>
        /// Whether or not an attack has landed during the current state.  Used for combo engine.
        /// </summary>
        protected bool _attackLanded;

        protected Queue<string> _queuedInput;

        #endregion //Fields

        #region Properties

        #region Required Data Structures

        /// <summary>
        /// the global id of this instance of a base object
        /// </summary>
        public uint Id { get; private set; }

        /// <summary>
        /// the id of this base object in teh player queue that owns it
        /// </summary>
        public int QueueId { get; private set; }

        /// <summary>
        /// The type of thing this is
        /// </summary>
        public string ObjectType { get; private set; }

        /// <summary>
        /// the animation container for this dude
        /// </summary>
        public AnimationContainer AnimationContainer { get; private set; }

        /// <summary>
        /// The state machine and state actions for this dude
        /// </summary>
        public IStateContainer States { get; set; }

        /// <summary>
        /// The player queue that owns this object
        /// </summary>
        public PlayerQueue PlayerQueue { get; set; }

        /// <summary>
        /// thing for managing all the collisions, hits for this dude
        /// </summary>
        public BasePhysicsContainer Physics { get; set; }

        /// <summary>
        /// drawlists used to draw the main character
        /// </summary>
        protected DrawList DrawList { get; set; }

        /// <summary>
        /// the garment manager for this guy, used to make life easier
        /// </summary>
        public GarmentManager Garments { get; protected set; }

        public string Name { get; set; }

        #endregion //Required Data Structures

        #region State Data

        /// <summary>
        /// Reference to a clock that synchronizes all the different clocks in the dude
        /// </summary>
        public HitPauseClock CharacterClock { get; protected set; }

        /// <summary>
        /// List of this dude's currently active attacks
        /// </summary>
        public TimedActionList<CreateAttackAction> CurrentAttacks { get; set; }

        /// <summary>
        /// If this dude is in a blocking state, this is a reference to that state
        /// </summary>
        public TimedActionList<BlockAction> CurrentBlocks { get; set; }

        /// <summary>
        /// If this timer is running, it means attacks don't hit
        /// </summary>
        public TimedActionList<ShieldAction> ShieldActions { get; set; }

        /// <summary>
        /// evasion timer, if this is running it means there are no push collsions
        /// </summary>
        public CountdownTimer EvasionTimer { get; protected set; }

        /// <summary>
        /// pointer to the current "throw" action (this dude is being thrown)
        /// </summary>
        public CreateThrowAction CurrentThrow { get; set; }

        /// <summary>
        /// When this timer runs out, 
        /// check the CharacterTrail object to see if we should drop another character image
        /// </summary>
        public CountdownTimer TrailTimer { get; private set; }

        /// <summary>
        /// pointer to the current "character trail" object
        /// </summary>
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

        /// <summary>
        /// Acceleration to apply to this dude for the current state
        /// </summary>
        public ConstantAccelerationAction AccelAction { get; set; }

        /// <summary>
        /// Decceleration to apply to this dude for the current state
        /// </summary>
        public ConstantDeccelerationAction DeccelAction { get; set; }

        /// <summary>
        /// The last player to attack this guy.  Used to calculate points when someone dies.
        /// </summary>
        public PlayerQueue LastAttacker { get; protected set; }

        /// <summary>
        /// a list of all the current particle effect emitters launched from state actions
        /// Used to kill particle emitters when state changes
        /// </summary>
        public List<Emitter> Emitters { get; private set; }

        /// <summary>
        /// a list of all the current point lights launched from state actions
        /// Used to kill lights when state changes
        /// </summary>
        public List<FlarePointLight> Lights { get; private set; }

        #endregion //State Data

        #region Positional Data

        /// <summary>
        /// How tall this character is (pixels)
        /// </summary>
        protected float _height;
        public float Height
        {
            get { return (_height * _scale); }
        }

        /// <summary>
        /// How big to draw, do physics at
        /// </summary>
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

        /// <summary>
        /// The current color of this dude
        /// </summary>
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

        /// <summary>
        /// this dude's position
        /// </summary>
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

        /// <summary>
        /// this dude's orientation
        /// </summary>
        public bool Flip { get; set; }

        /// <summary>
        /// the velocity vector of this object
        /// </summary>
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

        /// <summary>
        /// rotation to apply to this dude for the current state
        /// stored in radians/second
        /// </summary>
        public float RotationPerSecond { get; set; }

        /// <summary>
        /// The current rotation of this dude.
        /// </summary>
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

        /// <summary>
        /// initialize static member variables
        /// </summary>
        static BaseObject()
        {
            _idCounter = 0;
        }

        /// <summary>
        /// hello, standard constructor!
        /// </summary>
        /// <param name="eType">the type of this object</param>
        /// <param name="clock">a character clock.</param>
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

        /// <summary>
        /// Constructor for replacing a network player when they leave the game
        /// </summary>
        /// <param name="rHuman">the dude to be replaced, copy all his shit</param>
        public BaseObject(GameObjectType gamGameObjectType, BaseObject human)
        {
            //grab all this shit
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

        /// <summary>
        /// Replace all the base object pointers in this dude to point to a replacement object
        /// </summary>
        /// <param name="myBot">the replacement dude</param>
        public virtual void ReplaceOwner(PlayerObject myBot)
        {
            //should only be called in the child classes!
            Debug.Assert(false);
        }

        /// <summary>
        /// Reset the object for game start, character death
        /// </summary>
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

            //kill all the particle effects and clear out that list
            foreach (var emitter in Emitters)
            {
                emitter.EmitterTimer.Stop();
            }
            Emitters.Clear();

            //kill all the lights and clear out that list
            foreach (var light in Lights)
            {
                light.Kill();
            }
            Lights.Clear();
        }

        public virtual void Update()
        {
            //update all our clocks
            EvasionTimer.Update(CharacterClock);
            TrailTimer.Update(CharacterClock);

            //update the garments of this dude
            Garments.Update(CharacterClock);

            //update the state actions of this dude
            States.ExecuteActions(CharacterClock);

            UpdateEmitters();

            UpdateAnimation();
        }

        /// <summary>
        /// update the animation container.
        /// </summary>
        public virtual void UpdateAnimation()
        {
            //update the animations
            AnimationContainer.Update(CharacterClock, Position, Flip, CurrentRotation, false);
        }

        /// <summary>
        /// Clear out all the dead particle emitters
        /// </summary>
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

        /// <summary>
        /// Clear out all the dead particle emitters
        /// </summary>
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

        /// <summary>
        /// Do all the specific processing to get player input.
        /// For human players, this means getting info from the controller.
        /// For AI players, this means reacting to info in the list of "bad guys"
        /// Also can ignore attack input to have times when players can move but not attack, like at beginning of match
        /// </summary>
        /// <param name="controller">the controller for this player (bullshit and ignored for AI)</param>
        /// <param name="listBadGuys">list of all the players (ignored for human players)</param>
        /// <param name="ignoreAttackInput">If true, the object should only move and not attack anything</param>
        public virtual void GetPlayerInput(InputWrapper controller, List<IPlayerQueue> listBadGuys, bool ignoreAttackInput)
        {
        }

        /// <summary>
        /// update an input wrapper
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="input"></param>
        public virtual void UpdateInput(InputWrapper controller, IInputState input)
        {
        }

        public virtual void CheckHardCodedStates()
        {
            //TODO: move all this hardcode states junk into update

            //Apply acceleration to the character
            Accelerate();

            //apply decceleration to the character
            Deccelerate();

            //rorate the object
            ApplyRotation();
        }

        public void UpdateRagdoll()
        {
            AnimationContainer.UpdateRagdoll();
        }

        /// <summary>
        /// Add an attack to this dude's list of active attacks
        /// </summary>
        /// <param name="attackAction">the attack action to perform</param>
        public void AddAttack(CreateAttackAction attackAction)
        {
            CurrentAttacks.AddAction(attackAction, CharacterClock);
        }

        /// <summary>
        /// Send a message to this dudes state machine
        /// </summary>
        /// <param name="iMessage">the message to send to the state machine</param>
        /// <returns>bool: whether or not this dude changed states</returns>
        public bool SendStateMessage(string message)
        {
            //send the message to the state machine
            return States.SendStateMessage(message);
        }

        public void ForceStateChange(string state)
        {
            //force the state change in the state machine
            States.ForceStateChange(state);
        }

        /// <summary>
        /// Call this when the state changes to reset everything for the new state
        /// </summary>
        protected virtual void StateChanged(object sender, HybridStateChangeEventArgs eventArgs)
        {
            //was this a turn around message?
            if (States.CurrentState == "TurningAround" || States.CurrentState == "AirTurningAround")
            {
                Flip = !Flip;
            }

            //clear the attacks
            CurrentAttacks.Reset();

            //clear the blocks
            CurrentBlocks.Reset();
            ShieldActions.Reset();

            //clear the evades
            EvasionTimer.Stop();

            //clear the trail action
            TrailTimer.Stop();
            TrailAction = null;

            //clear the accel & deccel
            AccelAction = null;
            DeccelAction = null;
            RotationPerSecond = 0.0f;

            //remove any state specific garments
            Garments.Reset();

            //make sure to update this dude, 
            //because projectiles are activated in the player's update loop and placed in front of them in the update loop
            if (this is ProjectileObject)
            {
                AnimationContainer.Update(CharacterClock, Position, Flip, CurrentRotation, false);
            }
            _attackLanded = false;

            //kill all the particle effects and clear out that list
            foreach (var emitter in Emitters)
            {
                emitter.EmitterTimer.Stop();
            }
            Emitters.Clear();

            //kill all the lights and clear out that list
            foreach (var light in Lights)
            {
                light.Kill();
            }
            Lights.Clear();
        }

        public virtual void CheckCollisions(BaseObject badGuy)
        {
            //make sure not to check collisions against ourselves
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
            //set "attack landed" flag for this state for combo engine
            var player = AttackLanded();

            if (!otherObject.Hits[(int)HitType.Attack].Active || (attackAction.Damage > otherObject.Hits[(int)HitType.Attack].Strength))
            {
                //i just punched the other object

                //am I facing left or right?
                var direction = attackAction.Direction;
                if (Flip)
                {
                    direction.X *= -1.0f;
                }

                //the base object should be the player if this object is a projectile
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
            //set "attack landed" flag for this state for combo engine
            var rPlayer = AttackLanded();

            //my weapon just collided with that other dude's weapon

            //am I facing left or right?
            var direction = attackAction.Direction;
            if (Flip)
            {
                direction.X *= -1.0f;
            }

            //the base object should be the player if this object is a projectile
            otherObject.Hits[(int)HitType.Weapon].Set(direction, attackAction, attackAction.Damage, HitType.Weapon, rPlayer, firstCollisionPoint);
        }

        /// <summary>
        /// i just attacked another dude but he blocked it
        /// </summary>
        /// <param name="otherObject"></param>
        /// <param name="attackAction"></param>
        /// <param name="firstCollisionPoint"></param>
        /// <param name="secondCollisionPoint"></param>
        public virtual void BlockResponse(BasePhysicsContainer otherObject,
            CreateAttackAction attackAction,
            BlockAction otherDudesAction,
            Vector2 firstCollisionPoint,
            Vector2 secondCollisionPoint)
        {
            //set "attack landed" flag for this state for combo engine
            var player = AttackLanded();

            if (!otherObject.Hits[(int)HitType.Block].Active || (attackAction.Damage > otherObject.Hits[(int)HitType.Block].Strength))
            {
                //i just punched the other object

                //am I facing left or right?
                var direction = attackAction.Direction;
                if (Flip)
                {
                    direction.X *= -1.0f;
                }

                //the base object should be the player if this object is a projectile
                otherObject.Hits[(int)HitType.Block].Set(direction, attackAction, attackAction.Damage, HitType.Attack, player, firstCollisionPoint);

                //perform all the success actions for the BLOCKING action not the ATTACKING action!
                otherDudesAction.ExecuteSuccessActions();
            }
        }

        #endregion //Collision Responses

        /// <summary>
        /// Remove an attack from the list
        /// </summary>
        /// <param name="attackIndex"></param>
        /// <returns>True if it was able to remove the attack.</returns>
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
            //do boundary hits here in the base class
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

            ////remove finished attacks from the list
            //int i = 0;
            //while (i < CurrentAttacks.Count)
            //{
            //	if (CurrentAttacks[i].DoneTime <= CharacterClock.CurrentTime)
            //	{
            //		CurrentAttacks.RemoveAt(i);
            //	}
            //	else
            //	{
            //		i++;
            //	}
            //}

            //remove finished blocks from list
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

        /// <summary>
        /// add all the data for this dude to the camera
        /// </summary>
        /// <param name="camera"></param>
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

        /// <summary>
        /// called when this object lands an attack on another object
        /// Set the attack landed flag in the owner character for the combo engine
        /// </summary>
        /// <returns>The player who landed the attack.</returns>
        public virtual BaseObject AttackLanded()
        {
            _attackLanded = true;
            return this;
        }

        #region Rendering

        /// <summary>
        /// Check whether or not this dude should render a character trail
        /// </summary>
        /// <returns>bool: whether or not this dude needs to render a character trail</returns>
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

        /// <summary>
        /// Do the actual drawing of the dude
        /// </summary>
        /// <param name="renderer"></param>
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

        /// <summary>
        /// Get how far other objects need to stay away from this dude
        /// </summary>
        /// <returns>float, either part of the height or the distance to the edge of the nearest attack</returns>
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

        /// <summary>
        /// Get how far other objects are before we dont care about them
        /// </summary>
        /// <returns>float, part of the height or the distance to the edge of the nearest attack</returns>
        public float MaxDistance()
        {
            float maxDistance = Height * 0.55f;

            //no attacks, return the forward edge of the character
            return maxDistance;
        }

        /// <summary>
        /// fucntion to get the 'direction' this dude wants to go.
        /// for players this will be thumbstick direction
        /// for ai this will be direction they want to go
        /// for projectile this will be direction player thumbstick was held when rpojectile was shot
        /// </summary>
        /// <returns></returns>
        virtual public Vector2 Direction()
        {
            return Vector2.Zero;
        }

        /// <summary>
        /// Kill this dude!
        /// </summary>
        public virtual void KillPlayer()
        {
        }

        public override string ToString()
        {
            return Name;
        }

        #region Tools

        /// <summary>
        /// Get a list of all the weapon
        /// </summary>
        /// <param name="listWeapons"></param>
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

        /// <summary>
        /// Given an xml node, parse the contents.
        /// Override in child classes to read object-specific node types.
        /// </summary>
        /// <param name="model">the xml data to read</param>
        /// <param name="engine">the engine we are using to load</param>
        /// <param name="messageOffset">the message offset of this object's state machine</param>
        /// <returns></returns>
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