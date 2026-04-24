using GameTimer;
using HadoukInput;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace GameDonkeyLib
{
    public abstract class AIController
    {
        #region Properties

        protected PlayerObject Player { get; set; }
        private CountdownTimer UpdateTimer { get; set; }
        private float _updateDelta;
        private float UpdateDelta
        {
            get
            {
                return _updateDelta;
            }
            set
            {
                _updateDelta = value;
                UpdateAimDelta = UpdateDelta * 0.3f;
            }
        }
        private CountdownTimer AimTimer { get; set; }
        private float UpdateAimDelta { get; set; }

        static private Random _random = new Random(DateTime.Now.Millisecond);

        protected float HalfHeight { get; private set; }

        public int Difficulty
        {
            get { return ConvertAIToInt(); }
            set
            {
                switch (value)
                {
                    case 9: { UpdateDelta = 0.1f; } break;
                    case 8: { UpdateDelta = 0.2f; } break;
                    case 7: { UpdateDelta = 0.3f; } break;
                    case 6: { UpdateDelta = 0.4f; } break;
                    case 5: { UpdateDelta = 0.55f; } break;
                    case 4: { UpdateDelta = 0.7f; } break;
                    case 3: { UpdateDelta = 0.85f; } break;
                    case 2: { UpdateDelta = 1.0f; } break;
                    case 1: { UpdateDelta = 1.5f; } break;
                    default: { UpdateDelta = -1.0f; } break;
                }
            }
        }
        private Vector2 _direction;
        public Vector2 Direction
        {
            get
            {
                return _direction;
            }
            set
            {
                _direction = value;
            }
        }

        protected abstract int AttackDistance { get; }

        protected abstract int DefendDistance { get; }

        protected abstract float AttackPause { get; }

        private CountdownTimer AttackTimer { get; set; }

        protected BaseObject BadGuy { get; set; }

        protected Vector2 BadGuyDistance { get; set; }

        protected virtual bool TargetProjectiles => true;

        #endregion //Properties

        #region Methods

        public AIController(PlayerObject player)
        {
            Player = player;
            UpdateTimer = new CountdownTimer();
            AimTimer = new CountdownTimer();
            AttackTimer = new CountdownTimer();

            UpdateDelta = 0.25f;

            HalfHeight = Math.Min(Player.Height * 0.5f, AttackDistance);
        }

        public virtual void Update()
        {
            UpdateTimer.Update(Player.CharacterClock);
            AimTimer.Update(Player.CharacterClock);
            AttackTimer.Update(Player.CharacterClock);
        }
        public void GetPlayerInput(List<IPlayerQueue> listBadGuys, bool ignoreAttackInput)
        {
            // //TODO: check if the character is dead
            // if (Player.CheckIfDead())
            // {
            // 	SendDeathMessage();
            // }

            //check if we should update the target
            if (!AimTimer.HasTimeRemaining && (0.0f <= UpdateAimDelta))
            {
                AimTimer.Start(UpdateAimDelta);

                //loop through the "bad guys" and select a target
                BadGuy = null;
                BadGuyDistance = Vector2.Zero;
                for (var i = 0; i < listBadGuys.Count; i++)
                {
                    //first make sure this isn't me!
                    if (Player.QueueId == listBadGuys[i].QueueId)
                    {
                        continue;
                    }

                    //go through ALL the active objects in the player queue so AI will react correctly to projectiles
                    for (var j = 0; j < listBadGuys[i].Active.Count; j++)
                    {
                        //Check if this object is targettable and if we even want to target it
                        if (listBadGuys[i].Active[j].Targettable &&
                            (TargetProjectiles || !(listBadGuys[i].Active[j] is ProjectileObject)))
                        {
                            //get the distance to this dude
                            var distance = listBadGuys[i].Active[j].Position - Player.Position;
                            if ((null == BadGuy) || (BadGuyDistance.LengthSquared() > distance.LengthSquared()))
                            {
                                BadGuy = listBadGuys[i].Active[j];
                                BadGuyDistance = distance;
                            }
                        }
                    }
                }

                if (null != BadGuy && BadGuyDistance.LengthSquared() > 0f)
                {
                    //set the direction
                    Direction = Vector2.Normalize(BadGuyDistance);
                    _direction.Y = _direction.Y * -1f;
                }
            }

            //check if we should update the AI
            if (!UpdateTimer.HasTimeRemaining && (0.0f <= UpdateDelta))
            {
                //restart the timer and run the AI update loop
                UpdateTimer.Start(UpdateDelta);

                if (null == BadGuy || BadGuyDistance.LengthSquared() == 0f)
                {
                    //if AI wins a stock match, there won't be any bad guys
                    return;
                }

                //react to the target

                //do i need to turn around?
                if (BadGuyDistance.X <= 0.0f)
                {
                    //the bad guy is to the left of me
                    if (!Player.Flip)
                    {
                        SendTurnAroundMessage();
                    }
                }
                else
                {
                    //the BadGuyDistance guy is to the right
                    if (Player.Flip)
                    {
                        SendTurnAroundMessage();
                    }
                }

                //is the target attacking?
                var blocking = false;
                if (BadGuy.States.IsCurrentStateAttack() && BadGuyDistance.LengthSquared() <= (DefendDistance * DefendDistance))
                {
                    //select a defensive option
                    blocking = SelectDefensiveOption();
                }

                var attacking = false;
                if (!blocking)
                {
                    //If we aren't trying to block an attack and the target is in distance, take a swing at them.
                    var distanceSquared = BadGuyDistance.LengthSquared();
                    if (distanceSquared <= (AttackDistance * AttackDistance) &&
                        !ignoreAttackInput &&
                        !(BadGuy is ProjectileObject))
                    {
                        attacking = true;
                        if (!AttackTimer.HasTimeRemaining)
                        {
                            //the target must be close! try to attack the target
                            if (SelectOffensiveOption(distanceSquared))
                            {
                                AttackTimer.Start(AttackPause);
                            }
                        }
                    }
                }

                if (!attacking && !blocking)
                {
                    //shoudl i move towards the target?
                    if (BadGuyDistance.LengthSquared() > (HalfHeight * HalfHeight))
                    {
                        //the bad guy is to the left or right, move towards the target
                        SendWalkMessage();
                    }
                    else if (IsWalking())
                    {
                        SendDoneMessage();
                    }

                    //the target is far away, but is it above me?
                    if (BadGuyDistance.Y < (-2.0f * HalfHeight))
                    {
                        //teh bad guy is waaay above me, super jump at them
                        SendHighJumpMessage();
                    }
                    else if (BadGuyDistance.Y < (-1.0f * HalfHeight))
                    {
                        //jump at the target
                        SendJumpMessage();
                    }
                }
            }
        }

        protected abstract void SendTurnAroundMessage();

        protected abstract void SendWalkMessage();

        protected abstract bool IsWalking();

        protected abstract void SendDoneMessage();

        protected abstract void SendHighJumpMessage();

        protected abstract void SendJumpMessage();

        protected abstract bool SelectDefensiveOption();

        //TODO: should i block or evade?
        //if ((g_Random.Next() % 2) == 0)
        //{
        //    //block
        //    Debug.Assert(-1 != BlockMessage);
        //    SendAttackMessage(BlockMessage);
        //}
        //else
        //{
        //    //evade
        //    Debug.Assert(-1 != DashMessage);
        //    SendAttackMessage(DashMessage);
        //}


        protected abstract bool SelectOffensiveOption(float distanceSquared);
        //int iMin = (TurnAroundMessage - m_States.StateMachine.MessageOffset) + 1;
        //int iMax = m_States.StateMachine.NumMessages - iMin;

        //int iAttack = ((g_Random.Next() % iMax) + iMin);
        //Debug.Assert(iAttack >= 0);
        //Debug.Assert(iAttack > (TurnAroundMessage - m_States.StateMachine.MessageOffset));
        //Debug.Assert(iAttack < m_States.StateMachine.NumMessages);

        //SendAttackMessage(iAttack + m_States.StateMachine.MessageOffset);

        //SendAttackMessage((int)EState.Quick);

        protected abstract void SendDeathMessage();
        //protected override void SendAttackMessage(int iNextMoov)
        //{
        //    if (0.0f <= m_fUpdateDelta)
        //    {
        //        base.SendAttackMessage(iNextMoov);
        //    }
        //}
        //public override void CheckHardCodedStates()
        //{
        //	var currentState = States.CurrentState;
        //if (m_bMoveTowards)
        //{
        //    //only check for standing, walking, falling states
        //    if ((iCurrentState == WalkingState) ||
        //        FallingState ||
        //        (iCurrentState == JumpingState) ||
        //        (iCurrentState == HighJumpState))
        //    {
        //        Accelerate();
        //    }
        //    else
        //    {
        //        SendStateMessage(WalkMessage);
        //    }
        //}
        //else
        //{
        //    //user is no longer holding the forward direction
        //    if (iCurrentState == WalkingState)
        //    {
        //        SendStateMessage(DoneMessage);
        //    }
        //    else if (!States.IsStateAttack(iCurrentState) && 
        //        (StunnedState != iCurrentState) &&
        //        !DashState)
        //    {
        //        //move the x velocity to 0
        //        Deccelerate();
        //    }
        //}

        //if (BlockingState)
        //{
        //    //check the block button
        //    if (!m_bBlocking)
        //    {
        //        SendStateMessage(DoneMessage);
        //    }
        //}

        //	base.CheckHardCodedStates();
        //}
        public int ConvertAIToInt()
        {
            if ((UpdateDelta >= 0.0f) && (UpdateDelta < 0.2f))
            {
                return 9;
            }
            else if ((UpdateDelta >= 0.2f) && (UpdateDelta < 0.3f))
            {
                return 8;
            }
            else if ((UpdateDelta >= 0.3f) && (UpdateDelta < 0.4f))
            {
                return 7;
            }
            else if ((UpdateDelta >= 0.4f) && (UpdateDelta < 0.55f))
            {
                return 6;
            }
            else if ((UpdateDelta >= 0.55f) && (UpdateDelta < 0.7f))
            {
                return 5;
            }
            else if ((UpdateDelta >= 0.7f) && (UpdateDelta < 0.85f))
            {
                return 4;
            }
            else if ((UpdateDelta >= 0.85f) && (UpdateDelta < 1.0f))
            {
                return 3;
            }
            else if ((UpdateDelta >= 1.0f) && (UpdateDelta < 1.5f))
            {
                return 2;
            }
            else if ((UpdateDelta >= 1.5f) && (UpdateDelta < 2.0))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public void ChangeAIDifficulty(bool increase)
        {
            if (UpdateDelta <= 0.1f)
            {
                if (increase)
                {
                }
                else
                {
                    UpdateDelta = 0.2f;
                }
            }
            else if ((UpdateDelta >= 0.2f) && (UpdateDelta < 0.3f))
            {
                if (increase)
                {
                    UpdateDelta = 0.1f;
                }
                else
                {
                    UpdateDelta = 0.3f;
                }
            }
            else if ((UpdateDelta >= 0.3f) && (UpdateDelta < 0.4f))
            {
                if (increase)
                {
                    UpdateDelta = 0.2f;
                }
                else
                {
                    UpdateDelta = 0.4f;
                }
            }
            else if ((UpdateDelta >= 0.4f) && (UpdateDelta < 0.55f))
            {
                if (increase)
                {
                    UpdateDelta = 0.3f;
                }
                else
                {
                    UpdateDelta = 0.55f;
                }
            }
            else if ((UpdateDelta >= 0.55f) && (UpdateDelta < 0.7f))
            {
                if (increase)
                {
                    UpdateDelta = 0.4f;
                }
                else
                {
                    UpdateDelta = 0.7f;
                }
            }
            else if ((UpdateDelta >= 0.7f) && (UpdateDelta < 0.85f))
            {
                if (increase)
                {
                    UpdateDelta = 0.55f;
                }
                else
                {
                    UpdateDelta = 0.85f;
                }
            }
            else if ((UpdateDelta >= 0.85f) && (UpdateDelta < 1.0f))
            {
                if (increase)
                {
                    UpdateDelta = 0.7f;
                }
                else
                {
                    UpdateDelta = 1.0f;
                }
            }
            else if ((UpdateDelta >= 1.0f) && (UpdateDelta < 1.5f))
            {
                if (increase)
                {
                    UpdateDelta = 0.85f;
                }
                else
                {
                    UpdateDelta = 1.5f;
                }
            }
            else if ((UpdateDelta >= 1.5f) && (UpdateDelta < 2.0f))
            {
                if (increase)
                {
                    UpdateDelta = 1.0f;
                }
                else
                {
                    UpdateDelta = 2.0f;
                }
            }
            else
            {
                if (increase)
                {
                    UpdateDelta = 1.5f;
                }
                else
                {
                }
            }
        }

        #endregion //Methods
    }
}