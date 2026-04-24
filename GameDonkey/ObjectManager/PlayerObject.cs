using GameTimer;
using HadoukInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GameDonkeyLib
{
    public class PlayerObject : BaseObject
    {
        #region Properties
        public int ComboCounter { get; private set; }
        protected virtual float HitPause => 0.2f;
        public Texture2D Portrait { get; protected set; }
        protected Vector2 ThumbstickDirection;

        public event EventHandler<HealthEventArgs> HealthChangedEvent;

        public AIController AI { get; set; }

        #endregion //Properties

        #region Methods

        public PlayerObject(HitPauseClock clock, int queueId, string name)
            : base(GameObjectType.Human, clock, queueId, name)
        {
            //init is called by the base class, which will set everything up
        }

        public PlayerObject(GameObjectType gameObjectType, HitPauseClock clock, int queueId, string name)
            : base(gameObjectType, clock, queueId, name)
        {
            //init is called by the base class, which will set everything up
        }
        public override void ReplaceOwner(PlayerObject myBot)
        {
            //replace in the state container
            States.ReplaceOwner(myBot);

            //replace in the physics
            Physics.ReplaceOwner(myBot);
        }

        protected override void Init()
        {
            ThumbstickDirection = Vector2.Zero;
            States = new StateContainer();
            States.StateChangedEvent += this.StateChanged;
            Physics = new PlayerPhysicsContainer(this);

            Reset();
        }
        public override void Reset()
        {
            base.Reset();
        }

        public override void Update()
        {
            //update all our clocks
            EvasionTimer.Update(CharacterClock);
            TrailTimer.Update(CharacterClock);

            UpdateFallMessage();

            //update the garments of this dude
            Garments.Update(CharacterClock);

            //update the state actions of this dude
            States.ExecuteActions(CharacterClock);

            UpdateEmitters();

            //update the animations
            UpdateRotation();

            UpdateAnimation();
        }
        public override void UpdateInput(InputWrapper controller, IInputState input)
        {
            controller.Update(input, Flip);
        }
        public virtual void UpdateFallMessage()
        {
            //Overload in child classes!
        }
        public virtual void UpdateRotation()
        {
            //Overload in child classes!
        }

        public override void GetPlayerInput(InputWrapper controller, List<IPlayerQueue> listBadGuys, bool ignoreAttackInput)
        {
            if (null != AI)
            {
                AI.Update();
                AI.GetPlayerInput(listBadGuys, ignoreAttackInput);
                ThumbstickDirection = AI.Direction;
            }
            else
            {
                //get the thumbstick direction
                ThumbstickDirection = controller.Controller.Thumbsticks.LeftThumbstick.Direction;

                //get the next moov from the input
                var nextMoov = controller.GetNextMove();
                if (!ignoreAttackInput)
                {
                    SendAttackMessage(nextMoov);
                }
                else
                {
                    //if we are ignoring attack input, only send the message if it isn't an attack
                    if (!States.IsAttackMessage(nextMoov))
                    {
                        SendAttackMessage(nextMoov);
                    }
                }
            }
        }
        public override Vector2 Direction()
        {
            return new Vector2(ThumbstickDirection.X, ThumbstickDirection.Y * -1f);
        }
        protected virtual void SendAttackMessage(string nextMoov)
        {
            //am i currently in an attack state? 
            if (States.IsCurrentStateAttack())
            {
                //is the attack still currently active?
                if (States.IsAttackActive())
                {
                    //wait until the attack ends

                    if (!string.IsNullOrEmpty(nextMoov))
                    {
                        //add the move to the queue
                        _queuedInput.Enqueue(nextMoov);
                    }
                }

                //did that attack connect to a bad guy?
                else if (_attackLanded)
                {
                    //ok, the current attack is not active and there is queued input

                    while (0 < _queuedInput.Count)
                    {
                        //send the queued input to the state machine!
                        nextMoov = _queuedInput.Dequeue();
                        SendStateMessage(nextMoov);
                    }
                }
            }
            else
            {
                //ok character is in neutral state

                if (!string.IsNullOrEmpty(nextMoov))
                {
                    //ok not in an attack state, no queued input
                    SendStateMessage(nextMoov);
                }
                else
                {
                    //clear out the queued input
                    _queuedInput.Clear();
                }
            }
        }

        #region Hit Response

        public override void HitResponse(IGameDonkey engine)
        {
            //iterate through the hits, parsing as we go
            for (var i = 0; i < Physics.Hits.Length; i++)
            {
                if (Physics.Hits[i].Active)
                {
                    switch ((HitType)i)
                    {
                        case HitType.Attack:
                            {
                                //is this a grab or an attack?
                                if (Physics.Hits[i].IsThrow)
                                {
                                    //process grab hit
                                    RespondToGrab(Physics.Hits[i]);
                                }
                                else
                                {
                                    //process attack hit
                                    RespondToAttack(Physics.Hits[i], engine);
                                }
                            }
                            break;

                        case HitType.Ground:
                        case HitType.Ceiling:
                        case HitType.LeftWall:
                        case HitType.RightWall:
                            {
                                //taken care of in the base class
                            }
                            break;

                        case HitType.Push:
                            {
                                RespondToPushHit(Physics.Hits[i]);
                            }
                            break;

                        case HitType.Weapon:
                            {
                                RespondToWeaponHit(Physics.Hits[i], engine);
                            }
                            break;

                        case HitType.Block:
                            {
                                RespondToBlockedAttack(Physics.Hits[i], engine);
                            }
                            break;
                    }
                }
            }

            //Move the character
            if (null != CurrentThrow)
            {
                //did the other guy let us go?
                if (CurrentThrow.TimeToRelease <= CharacterClock.CurrentTime)
                {
                    //send the 'done' message
                    SendStateMessage("Done");

                    //set the velocity
                    var throwVelocity = CurrentThrow.Direction;

                    //flip the direction?
                    if (!Flip)
                    {
                        throwVelocity.X *= -1.0f;
                    }

                    Velocity = throwVelocity;

                    //null out that pointer
                    CurrentThrow = null;
                }
            }

            base.HitResponse(engine);
        }

        protected override void RespondToGroundHit(Hit groundHit, IGameDonkey engine)
        {
            //move the player UP out of the floor
            _position.Y += (groundHit.Strength * groundHit.Direction.Y);

            if (States.CurrentState == "Stunned")
            {
                //if the player is stunned, bounce them up in the air
                _velocity.Y = -1.0f * Math.Abs(Velocity.Y);

                engine.PlayParticleEffect(DefaultParticleEffect.StunnedBounce,
                    Velocity,
                    groundHit.Position,
                    Color.White);

                //add camera shake
                engine.AddCameraShake(0.2f);

                //TODO: make a sound for hitting boundary while stunned
            }

            if (0.0f < Velocity.Y)
            {
                //if the player's velocity is +y, it is set to 0
                _velocity.Y = 0.0f;

                if (States.StateMachine.Messages.Contains("HitGround"))
                {
                    SendStateMessage("HitGround");
                }
            }
        }

        protected override void RespondToCeilingHit(Hit groundHit, IGameDonkey engine)
        {
            //move the player down out of the ceiling
            _position.Y += (groundHit.Strength * groundHit.Direction.Y);

            //always bounce the player out of a ceiling hit
            _velocity.Y = -1.0f * Velocity.Y;

            if (States.CurrentState == "Stunned")
            {
                //add camera shake
                engine.AddCameraShake(0.2f);
            }

            //if the player's velocity is -y, it is set to 0
            if (Velocity.Y < 0.0f)
            {
                _velocity.Y = 0.0f;
            }
        }

        protected override void RespondToLeftWallHit(Hit groundHit, IGameDonkey engine)
        {
            //move the player UP out of the floor
            _position.X += (groundHit.Strength * groundHit.Direction.X);

            if (States.CurrentState == "Stunned")
            {
                //if the player is stunned, bounce them up in the air
                _velocity.X = -1.0f * Velocity.X;

                engine.PlayParticleEffect(DefaultParticleEffect.StunnedBounce,
                    Velocity,
                    groundHit.Position,
                    Color.White);

                //add camera shake
                engine.AddCameraShake(0.2f);

                //TODO: make a sound for hitting boundary while stunned
            }

            //if the player's velocity is -X, it is set to 0
            if (Velocity.X < 0.0f)
            {
                _velocity.X = 0.0f;
            }
        }

        protected override void RespondToRightWallHit(Hit groundHit, IGameDonkey engine)
        {
            //move the player UP out of the floor
            _position.X += (groundHit.Strength * groundHit.Direction.X);

            if (States.CurrentState == "Stunned")
            {
                //if the player is stunned, bounce them up in the air
                _velocity.X = -1.0f * Velocity.X;

                engine.PlayParticleEffect(DefaultParticleEffect.StunnedBounce,
                    Velocity,
                    groundHit.Position,
                    Color.White);

                //add camera shake
                engine.AddCameraShake(0.2f);

                //TODO: make a sound for hitting boundary while stunned
            }

            //if the player's velocity is +X, it is set to 0
            if (0 < Velocity.X)
            {
                _velocity.X = 0.0f;
            }
        }

        protected virtual void RespondToAttack(Hit attack, IGameDonkey engine)
        {
            //set this dude's last attacker to the other dude
            LastAttacker = attack.Attacker.PlayerQueue;

            if (IsShielded())
            {
                //do a block!
                ShieldActions.CurrentActions[0].ExecuteSuccessActions();
                RespondToBlockedAttack(attack, engine);
            }
            else if (EvasionTimer.RemainingTime <= 0.0f) //make sure the character is not evading
            {
                //if the player is already stunned, restart his state timer
                if (States.CurrentState == "Stunned")
                {
                    States.ForceStateChange("Stunned");
                }

                //add the damage
                HealthChanged(attack.Strength);
                TakeDamage(attack.Strength);

                //add the velocity
                Velocity = AttackedVector(attack);

                //send the state message
                SendStateMessage("Hit");

                //do a hit pause
                if (!attack.IsAoE)
                {
                    CharacterClock.AddHitPause(HitPause);
                    attack.Attacker.CharacterClock.AddHitPause(HitPause);
                }

                //add camera shake
                engine.AddCameraShake(0.25f);

                //add the hit spark
                engine.PlayParticleEffect(DefaultParticleEffect.HitSpark,
                    Vector2.Zero,
                    attack.Position,
                    attack.Attacker.PlayerQueue.PlayerColor);

                //add a hit cloud
                engine.PlayParticleEffect(DefaultParticleEffect.HitCloud,
                    Vector2.Zero,
                    attack.Position,
                    Color.Yellow);

                //shoot particles out of teh characters butt
                engine.PlayParticleEffect(DefaultParticleEffect.HitCloud,
                    Velocity * 1.5f,
                    attack.Position,
                    Color.Yellow);

                //play the hit noise
                if (null != attack.HitSound)
                {
                    attack.HitSound.Play();
                }
            }

            //clear out the rest of the hits so that the player isn't hit multiple times by the same attack
            Physics.Reset();
        }

        protected void HealthChanged(float attackStrength)
        {
            HealthChangedEvent?.Invoke(this, new HealthEventArgs(attackStrength));
        }

        protected virtual Vector2 AttackedVector(Hit attack)
        {
            var hitDirection = attack.Direction;

            //if this player is already stunned, strengthen the hit
            if (States.CurrentState == "Stunned")
            {
                //This player was already stunned, increment the combo counter
                ComboCounter++;

                //add the combo multiplier to the hit direction
                float multiplier = 1.0f + (0.3f * ComboCounter);
                hitDirection *= multiplier;
            }
            else
            {
                //this player was not stunned, reset the combo counter
                ComboCounter = 0;
            }

            //add the attacking player's velocity to the hit direction
            hitDirection += (attack.Attacker.Velocity * 0.5f);

            return hitDirection;
        }
        private void RespondToGrab(Hit grab)
        {
            //TODO: does any grab logic need to be performed?
        }

        protected virtual void RespondToPushHit(Hit push)
        {
            //push away from all push hits!
            var deltaVect = push.Direction * push.Strength;
            deltaVect.Y = 0.0f;
            Position += deltaVect;
        }

        protected virtual void RespondToWeaponHit(Hit weaponHit, IGameDonkey engine)
        {
            //set this dude's last attacker to the other dude
            LastAttacker = weaponHit.Attacker.PlayerQueue;

            //if this player has over 100% damage, double the strength of the hit
            var hitDirection = AttackedVector(weaponHit) * 0.5f;

            //add the velocity
            Velocity = hitDirection;

            //do a hit pause
            if (!weaponHit.IsAoE)
            {
                CharacterClock.AddHitPause(HitPause);
                weaponHit.Attacker.CharacterClock.AddHitPause(HitPause);
            }

            //add camera shake
            engine.AddCameraShake(0.08f);

            //do a special hit spark for weapon clash
            engine.PlayParticleEffect(DefaultParticleEffect.WeaponHit,
                    Velocity * 1.5f,
                    weaponHit.Position,
                    Color.White);
        }

        protected virtual void RespondToBlockedAttack(Hit attack, IGameDonkey engine)
        {
            //do a block!

            //add the velocity
            Velocity = AttackedVector(attack) * 0.9f;

            //do a hit pause
            if (!attack.IsAoE)
            {
                CharacterClock.AddHitPause(HitPause * 0.8f);
                attack.Attacker.CharacterClock.AddHitPause(HitPause * 0.8f);
            }

            //play the particle effect
            engine.PlayParticleEffect(DefaultParticleEffect.Block,
                new Vector2((attack.Attacker.Flip ? -400.0f : 400.0f), 0.0f),
                attack.Position,
                new Color(0, 255, 255));
        }

        #endregion //Hit Response

        public virtual void TakeDamage(float damage)
        {
        }

        #endregion //Methods

        #region File IO
        public override void ParseXmlData(BaseObjectModel model, IGameDonkey engine, ContentManager content)
        {
            PlayerObjectModel data = model as PlayerObjectModel;
            if (null == data)
            {
                throw new Exception("must pass PlayerObjectModel to PlayerObject.ParseXmlData");
            }

            //load player object stuff
            if ((null != data.Portrait) && (null != engine.Renderer.Content))
            {
                var textureInfo = engine.Renderer.LoadImage(data.Portrait);
                Portrait = textureInfo.Texture;
            }

            base.ParseXmlData(model, engine, content);
        }

        protected T FindAction<T>(IStateContainer container, string stateName, string actionName) where T : BaseAction
        {
            return container.Actions.GetStateActions(stateName).FindAction(actionName) as T;
        }

        #endregion //File IO
    }
}