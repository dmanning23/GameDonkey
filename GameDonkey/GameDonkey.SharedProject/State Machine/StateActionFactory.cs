using System;

namespace GameDonkeyLib
{
	/// <summary>
	/// this thing does a lot of sorting between different enums and state action objects, etc.
	/// </summary>
	public static class StateActionFactory
	{
		#region Methods

		public static BaseActionModel CreateActionModel(string actionType)
		{
			//convert to the enumeration
			var stateActionType = (EActionType)Enum.Parse(typeof(EActionType), actionType);

			switch (stateActionType)
			{
				case EActionType.AddGarment: { return new AddGarmentActionModel(); }
				case EActionType.AddVelocity: { return new AddVelocityActionModel(); }
				case EActionType.BlockState: { return new BlockingStateActionModel(); }
				case EActionType.CameraShake: { return new CameraShakeActionModel(); }
				case EActionType.ConstantAcceleration: { return new ConstantAccelerationActionModel(); }
				case EActionType.ConstantDecceleration: { return new ConstantDeccelerationActionModel(); }
				case EActionType.CreateAttack: { return new CreateAttackActionModel(); }
				case EActionType.CreateBlock: { return new CreateBlockActionModel(); }
				case EActionType.CreateHitCircle: { return new CreateHitCircleActionModel(); }
				case EActionType.CreateThrow: { return new CreateThrowActionModel(); }
				case EActionType.Deactivate: { return new DeactivateActionModel(); }
				case EActionType.Evade: { return new EvadeActionModel(); }
				case EActionType.KillPlayer: { return new KillPlayerActionModel(); }
				case EActionType.ParticleEffect: { return new ParticleEffectActionModel(); }
				case EActionType.PlayAnimation: { return new PlayAnimationActionModel(); }
				case EActionType.PlaySound: { return new PlaySoundActionModel(); }
				case EActionType.Projectile: { return new ProjectileActionModel(); }
				case EActionType.Rotate: { return new RotateActionModel(); }
				case EActionType.SendStateMessage: { return new SendStateMessageActionModel(); }
				case EActionType.SetVelocity: { return new SetVelocityActionModel(); }
				case EActionType.TargetRotation: { return new TargetRotationActionModel(); }
				case EActionType.Trail: { return new TrailActionModel(); }
				default: { throw new Exception($"unknown actionType: {actionType}"); }
			}
		}

		/// <summary>
		/// factory method to create the correct action given an action type
		/// </summary>
		/// <param name="actionType">the type of action to add</param>
		/// <param name="owner">the owner of this action list</param>
		/// <returns>IBaseAction: reference to the action that was created</returns>
		public static BaseAction CreateStateAction(BaseActionModel actionModel, BaseObject owner)
		{
			//get the correct action type
			switch (actionModel.ActionType)
			{
				case EActionType.AddGarment: { return new AddGarmentAction(owner, actionModel); }
				case EActionType.AddVelocity: { return new AddVelocityAction(owner, actionModel); }
				case EActionType.BlockState: { return new BlockingStateAction(owner, actionModel); }
				case EActionType.CameraShake: { return new CameraShakeAction(owner, actionModel); }
				case EActionType.ConstantAcceleration: { return new ConstantAccelerationAction(owner, actionModel); }
				case EActionType.ConstantDecceleration: { return new ConstantDeccelerationAction(owner, actionModel); }
				case EActionType.CreateAttack: { return new CreateAttackAction(owner, actionModel); }
				case EActionType.CreateBlock: { return new CreateBlockAction(owner, actionModel); }
				case EActionType.CreateHitCircle: { return new CreateHitCircleAction(owner, actionModel); }
				case EActionType.CreateThrow: { return new CreateThrowAction(owner, actionModel); }
				case EActionType.Deactivate: { return new DeactivateAction(owner, actionModel); }
				case EActionType.Evade: { return new EvadeAction(owner, actionModel); }
				case EActionType.KillPlayer: { return new KillPlayerAction(owner, actionModel); }
				case EActionType.ParticleEffect: { return new ParticleEffectAction(owner, actionModel); }
				case EActionType.PlayAnimation: { return new PlayAnimationAction(owner, actionModel); }
				case EActionType.PlaySound: { return new PlaySoundAction(owner, actionModel); }
				case EActionType.Projectile: { return new ProjectileAction(owner, actionModel); }
				case EActionType.Rotate: { return new RotateAction(owner, actionModel); }
				case EActionType.SendStateMessage: { return new SendStateMessageAction(owner, actionModel); }
				case EActionType.SetVelocity: { return new SetVelocityAction(owner, actionModel); }
				case EActionType.TargetRotation: { return new TargetRotationAction(owner, actionModel); }
				case EActionType.Trail: { return new TrailAction(owner, actionModel); }
				default: { throw new Exception($"unknown actionType: {actionModel.ActionType}"); }
			}
		}

		/// <summary>
		/// factory method to create the correct action given an action type
		/// </summary>
		/// <param name="actionType">the type of action to add</param>
		/// <param name="owner">the owner of this action list</param>
		/// <returns>IBaseAction: reference to the action that was created</returns>
		public static BaseAction CreateStateAction(EActionType actionType, BaseObject owner)
		{
			switch (actionType)
			{
				case EActionType.AddGarment: { return new AddGarmentAction(owner); }
				case EActionType.AddVelocity: { return new AddVelocityAction(owner); }
				case EActionType.BlockState: { return new BlockingStateAction(owner); }
				case EActionType.CameraShake: { return new CameraShakeAction(owner); }
				case EActionType.ConstantAcceleration: { return new ConstantAccelerationAction(owner); }
				case EActionType.ConstantDecceleration: { return new ConstantDeccelerationAction(owner); }
				case EActionType.CreateAttack: { return new CreateAttackAction(owner); }
				case EActionType.CreateBlock: { return new CreateBlockAction(owner); }
				case EActionType.CreateHitCircle: { return new CreateHitCircleAction(owner); }
				case EActionType.CreateThrow: { return new CreateThrowAction(owner); }
				case EActionType.Deactivate: { return new DeactivateAction(owner); }
				case EActionType.Evade: { return new EvadeAction(owner); }
				case EActionType.KillPlayer: { return new KillPlayerAction(owner); }
				case EActionType.ParticleEffect: { return new ParticleEffectAction(owner); }
				case EActionType.PlayAnimation: { return new PlayAnimationAction(owner); }
				case EActionType.PlaySound: { return new PlaySoundAction(owner); }
				case EActionType.Projectile: { return new ProjectileAction(owner); }
				case EActionType.Rotate: { return new RotateAction(owner); }
				case EActionType.SendStateMessage: { return new SendStateMessageAction(owner); }
				case EActionType.SetVelocity: { return new SetVelocityAction(owner); }
				case EActionType.TargetRotation: { return new TargetRotationAction(owner); }
				case EActionType.Trail: { return new TrailAction(owner); }
				default: { throw new Exception($"unknown actionType: {actionType}"); }
			}
		}

		#endregion //Methods
	}
}