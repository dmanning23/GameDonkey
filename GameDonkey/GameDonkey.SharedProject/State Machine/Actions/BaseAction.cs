using Microsoft.Xna.Framework.Content;
using System.Text;

namespace GameDonkeyLib
{
	/// <summary>
	/// The base interface for state machine actions
	/// </summary>
	public abstract class BaseAction
	{
		#region Properties

		/// <summary>
		/// the type of this action
		/// </summary>
		public EActionType ActionType { get; private set; }

		/// <summary>
		/// The game object that owns this action
		/// </summary>
		public BaseObject Owner { get; set; }

		/// <summary>
		/// whether or not this action has been run 
		/// </summary>
		public bool AlreadyRun { get; set; }

		public string Id { get; set; }

		/// <summary>
		/// the time from the start of the state that this action ocuurs
		/// </summary>
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

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public virtual bool Execute()
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

		#endregion //Methods
	}
}
