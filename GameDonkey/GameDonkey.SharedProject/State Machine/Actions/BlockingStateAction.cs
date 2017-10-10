using AnimationLib;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace GameDonkeyLib
{
	/// <summary>
	/// This is the state action used for when a state is a "blocking" state
	/// It runs until the state is exited.
	/// </summary>
	public class BlockingStateAction : TimedAction
	{
		#region Properties

		/// <summary>
		/// the name of the bone to use
		/// </summary>
		protected string _boneName;
		public string BoneName
		{
			get { return _boneName; }
			set
			{
				_boneName = value;

				//if the bone name is changed, it means the bone needs to be reset too...
				AttackBone = null;
			}
		}

		/// <summary>
		/// The bone this attack uses
		/// </summary>
		public Bone AttackBone { get; private set; }

		/// <summary>
		/// A list of actions that will be run if this action blocks an attack (sound effects, particle effects, etc)
		/// </summary>
		public List<BaseAction> SuccessActions { get; private set; }

		#endregion //Properties

		#region Initialization

		public BlockingStateAction(BaseObject owner) :
			base(owner, EActionType.BlockState)
		{
			SuccessActions = new List<BaseAction>();
		}

		public BlockingStateAction(BaseObject owner, BlockingStateActionModel actionModel) :
			base(owner, actionModel, actionModel.TimeDelta)
		{
			SuccessActions = new List<BaseAction>();
			for (int i = 0; i < actionModel.SuccessActions.Count; i++)
			{
				var stateAction = StateActionFactory.CreateStateAction(actionModel.SuccessActions[i], owner);
				SuccessActions.Add(stateAction);
			}
		}

		public BlockingStateAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as BlockingStateActionModel)
		{
		}

		public override void LoadContent(IGameDonkey engine, SingleStateContainer stateContainer, ContentManager content)
		{
			for (int i = 0; i < SuccessActions.Count; i++)
			{
				SuccessActions[i].LoadContent(engine, stateContainer, content);
			}
		}

		#endregion //Initialization

		#region Methods

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			//Check if the bone is set, if not try and find it...
			if (null == AttackBone)
			{
				AttackBone = Owner.Physics.FindWeapon(BoneName);
			}

			//reset teh success actions
			for (int i = 0; i < SuccessActions.Count; i++)
			{
				SuccessActions[i].AlreadyRun = false;
			}

			//add this action to the list of block states
			Owner.CurrentBlocks.AddAction(this, Owner.CharacterClock);

			return base.Execute();
		}

		public virtual PhysicsCircle GetCircle()
		{
			//return the first circle from this dude's image

			if (null == AttackBone)
			{
				return null;
			}

			//get the current image
			var image = AttackBone.GetCurrentImage();

			//hit bones and images must have one circle
			if ((null == image) || (image.Circles.Count < 1))
			{
				return null;
			}

			//get the circle
			return image.Circles[0]; ;
		}

		/// <summary>
		/// execute all the success actions after this attack lands
		/// </summary>
		/// <returns>bool: whether or not a state change occurred while this dude was running</returns>
		public bool ExecuteSuccessActions()
		{
			var result = false;
			for (int i = 0; i < SuccessActions.Count; i++)
			{
				if (SuccessActions[i].Execute())
				{
					result = true;
				}
			}

			return result;
		}

		#endregion //Methods
	}
}