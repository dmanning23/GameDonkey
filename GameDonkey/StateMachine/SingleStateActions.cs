using GameTimer;
using Microsoft.Xna.Framework.Content;

namespace GameDonkeyLib
{
    public class SingleStateActions : StateActionsList
    {
        #region Properties
        protected float ActiveTime { get; private set; }
        protected float RecoveryTime { get; private set; }
        public string StateName { get; set; }
        public bool IsAttack { get; private set; }

        #endregion //Properties

        #region Methods

        #region Initialization
        public SingleStateActions()
        {
        }

        public void LoadStateActions(SingleStateActionsModel actionModels, BaseObject owner, IStateContainer stateContainer)
        {
            StateName = actionModels.StateName;
            base.LoadStateActions(actionModels, owner, stateContainer);
        }

        public override void LoadContent(IGameDonkey engine, ContentManager content)
        {
            base.LoadContent(engine, content);

            //calculate "active" and "recovery" phases
            CalculateAttackTime();
        }

        #endregion //Initialization
        public void StateChange()
        {
            for (int i = 0; i < Actions.Count; i++)
            {
                Actions[i].Reset();
            }
        }

        private void CalculateAttackTime()
        {
            //does this state have any attack actions?
            for (int i = 0; i < Actions.Count; i++)
            {
                if ((Actions[i].ActionType == EActionType.CreateAttack) ||
                    (Actions[i].ActionType == EActionType.CreateThrow) ||
                    (Actions[i].ActionType == EActionType.CreateHitCircle))
                {
                    //set this state to an attack state
                    IsAttack = true;

                    //check if this is the end of the startup 
                    var attackAction = (CreateAttackAction)Actions[i];
                    if (0.0f == ActiveTime)
                    {
                        ActiveTime = attackAction.Time;
                    }

                    //check if this attack is teh recovery time
                    if (RecoveryTime < (attackAction.Time + attackAction.TimeDelta))
                    {
                        RecoveryTime = attackAction.Time + attackAction.TimeDelta;
                    }
                }
                else if (Actions[i].ActionType == EActionType.Projectile)
                {
                    //set this state to an attack state
                    IsAttack = true;

                    //check if this is the end of the startup 
                    var projectileAction = (ProjectileAction)Actions[i];
                    if (0.0f == ActiveTime)
                    {
                        ActiveTime = projectileAction.Time;
                    }

                    //check if this attack is teh recovery time
                    if (RecoveryTime < (projectileAction.Time))
                    {
                        RecoveryTime = projectileAction.Time;
                    }
                }
            }
        }

        public bool IsAttackActive(GameClock stateClock)
        {
            //the attacks are still active if the recovery time hasnt started
            return (stateClock.CurrentTime < RecoveryTime);
        }
        public void ReplaceOwner(BaseObject bot)
        {
            //replace in all the state actions
            for (int i = 0; i < Actions.Count; i++)
            {
                Actions[i].Owner = bot;
            }
        }

        public override string ToString()
        {
            return StateName;
        }

        #endregion //Methods
    }
}