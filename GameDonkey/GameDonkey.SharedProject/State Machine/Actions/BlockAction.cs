using AnimationLib;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace GameDonkeyLib
{
	/// <summary>
	/// This state is when the character blocks with one bone.
	/// </summary>
	public class BlockAction : ShieldAction
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

		public BlockAction(BaseObject owner) :
			base(owner, EActionType.Block)
		{
			
		}

		public BlockAction(BaseObject owner, BlockActionModel actionModel, IStateContainer container) :
			base(owner, actionModel, container)
		{
			BoneName = actionModel.BoneName;
		}

		public BlockAction(BaseObject owner, BaseActionModel actionModel, IStateContainer container) :
			this(owner, actionModel as BlockActionModel, container)
		{
		}

		public override void LoadContent(IGameDonkey engine, ContentManager content)
		{
			base.LoadContent(engine, content);
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
			SetAttackBone();

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

		public void SetAttackBone()
		{
			//Check if the bone is set, if not try and find it...
			if (null == AttackBone)
			{
				AttackBone = Owner.Physics.FindWeapon(BoneName);
			}
		}

		#endregion //Methods
	}
}