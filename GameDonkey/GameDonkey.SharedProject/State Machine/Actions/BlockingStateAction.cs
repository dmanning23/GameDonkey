using AnimationLib;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace GameDonkeyLib
{
	/// <summary>
	/// This is the state action used for when a state is a "blocking" state
	/// It runs until the state is exited.
	/// </summary>
	public class BlockingStateAction : CreateBlockAction
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

		#endregion //Properties

		#region Initialization

		public BlockingStateAction(BaseObject owner) :
			base(owner, EActionType.BlockState)
		{
			
		}

		public BlockingStateAction(BaseObject owner, BlockingStateActionModel actionModel) :
			base(owner, actionModel)
		{
			BoneName = actionModel.BoneName;
		}

		public BlockingStateAction(BaseObject owner, BaseActionModel actionModel) :
			this(owner, actionModel as BlockingStateActionModel)
		{
		}

		public override void LoadContent(IGameDonkey engine, SingleStateContainer stateContainer, ContentManager content)
		{
			base.LoadContent(engine, stateContainer, content);
		}

		#endregion //Initialization

		#region Methods

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			return base.Execute();
		}

		protected override void AddBlock()
		{
			//Check if the bone is set, if not try and find it...
			if (null == AttackBone)
			{
				AttackBone = Owner.Physics.FindWeapon(BoneName);
			}

			//add this action to the list of block states
			Owner.CurrentBlocks.AddAction(this, Owner.CharacterClock);
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

		#endregion //Methods
	}
}