using AnimationLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace GameDonkeyLib
{
	public class CreateAttackAction : TimedAction, IStateActionsList
	{
		#region Properties

		/// <summary>
		/// the name of the bone to use
		/// </summary>
		private string _boneName;
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
		/// The bone this attack uses.  Has to be a weapon bone!
		/// Starts out as null, and is set at runtime the first time this action is run.
		/// Since it can be a garment bone, these might not actually be in the model at startup
		/// </summary>
		public Bone AttackBone { get; protected set; }

		/// <summary>
		/// the vector to set another object to when this attack connects
		/// </summary>
		public ActionDirection ActionDirection { get; set; }
		public Vector2 Direction
		{
			get
			{
				return ActionDirection.GetDirection(Owner);
			}
		}

		public SoundEffect HitSound { get; private set; }

		/// <summary>
		/// A list of actions that will be run if this attack connects (sound effects, particle effects, etc)
		/// This list of actions is played whether the attack is blocked or not.
		/// </summary>
		private StateActionsList StateActionsList { get; set; }

		public List<BaseAction> Actions => StateActionsList.Actions;

		/// <summary>
		/// the amount of damage to deal when this attack connects
		/// </summary>
		public float Damage { get; set; }

		/// <summary>
		/// True if this attack can hit multiple enemies, false if only hits one
		/// </summary>
		public bool AoE { get; set; }

		#endregion //Properties

		#region Initialization

		public CreateAttackAction(BaseObject owner, EActionType actionType = EActionType.CreateAttack) :
			base(owner, actionType)
		{
			ActionDirection = new ActionDirection();
			StateActionsList = new StateActionsList();
		}

		public CreateAttackAction(BaseObject owner, CreateAttackActionModel actionModel, IStateContainer stateContainer) :
			base(owner, actionModel, actionModel.TimeDelta)
		{
			BoneName = actionModel.BoneName;
			Damage = actionModel.Damage;
			AoE = actionModel.AoE;
			ActionDirection = new ActionDirection(actionModel.Direction);

			StateActionsList = new StateActionsList();
			StateActionsList.LoadStateActions(actionModel.ActionModels, owner, stateContainer);
		}

		public CreateAttackAction(BaseObject owner, BaseActionModel actionModel, IStateContainer stateContainer) :
			this(owner, actionModel as CreateAttackActionModel, stateContainer)
		{
		}

		public override void LoadContent(IGameDonkey engine, ContentManager content)
		{
			StateActionsList.LoadContent(engine, content);
		}

		#endregion //Initialization

		#region Methods

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			SetAttackBone();

			//reset teh success actions
			for (var i = 0; i < StateActionsList.Actions.Count; i++)
			{
				StateActionsList.Actions[i].AlreadyRun = false;
			}

			//activate the attack
			DoneTime = Owner.CharacterClock.CurrentTime + TimeDelta;

			//add this actionto the list of attacks
			Owner.AddAttack(this);

			return base.Execute();
		}

		public void SetAttackBone()
		{
			//Check if the bone is set, if not try and find it...
			if (null == AttackBone)
			{
				AttackBone = Owner.Physics.FindWeapon(BoneName);
			}
		}

		public virtual void Update()
		{
			//nothing to do here, used in child classes
		}

		public virtual PhysicsCircle GetCircle()
		{
			//return the first circle from this dude's image

			//Check if the bone is set, if not try and find it...
			SetAttackBone();

			//the bone for this attack is in a garment that isnt being displayed
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
			var circle = image.Circles[0];

			return circle;
		}

		/// <summary>
		/// execute all the success actions after this attack lands
		/// </summary>
		/// <param name="characterHit">The dude that got nailed by this attack</param>
		/// <returns>bool: whether or not a state change occurred while this dude was running</returns>
		public virtual bool ExecuteSuccessActions(BaseObject characterHit)
		{
			var result = false;
			for (var i = 0; i < StateActionsList.Actions.Count; i++)
			{
				if (StateActionsList.Actions[i].Execute())
				{
					result = true;
				}
			}

			return result;
		}

		public BaseAction AddNewActionFromType(EActionType actionType, BaseObject owner, IGameDonkey engine, ContentManager content)
		{
			return StateActionsList.AddNewActionFromType(actionType, owner, engine, content);
		}

		public void LoadStateActions(StateActionsListModel actionModels, BaseObject owner, IStateContainer stateContainer)
		{
			StateActionsList.LoadStateActions(actionModels, owner, stateContainer);
		}

		public bool RemoveAction(BaseAction action)
		{
			return StateActionsList.RemoveAction(action);
		}

		public void Sort()
		{
			StateActionsList.Sort();
		}

		#endregion //Methods
	}
}