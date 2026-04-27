using Microsoft.Xna.Framework.Content;
using System.Text;

namespace GameDonkeyLib
{
    public abstract class BaseAction
    {
        #region Properties

        public EActionType ActionType { get; private set; }

        public BaseObject Owner { get; set; }

        public bool AlreadyRun { get; protected set; }

        public string Id { get; set; }

        public float Time { get; set; }

        #endregion //Properties

        #region Methods

        public BaseAction(BaseObject owner, EActionType actionType)
        {
            ActionType = actionType;
            Time = 0.0f;
            AlreadyRun = true;
            Owner = owner;
        }

        protected BaseAction(BaseObject owner, BaseActionModel actionModel) : this(owner, actionModel.ActionType)
        {
            Time = actionModel.Time;
            Id = actionModel.Id;
        }

        public abstract void LoadContent(IGameDonkey engine, ContentManager content);

        public virtual bool Execute(float currentTime)
        {
            AlreadyRun = true;
            return false;
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            if (!string.IsNullOrEmpty(Id))
            {
                result.Append($"{Id} ");
            }
            result.Append($"{Time.ToString()}: {ActionType.ToString()}");

            return result.ToString();
        }

        public virtual void Reset()
        {
            AlreadyRun = false;
        }

        #endregion //Methods
    }
}
