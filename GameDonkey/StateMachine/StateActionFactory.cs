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
			EActionType stateActionType;
			if ("Type" == actionType)
			{
				//legacy shit
				stateActionType = XMLTypeToType(actionType);
			}
			else
			{
				//convert to the enumeration
				stateActionType = (EActionType)Enum.Parse(typeof(EActionType), actionType);
			}

			switch (stateActionType)
			{
				case EActionType.AddGarment: { return new AddGarmentActionModel(); }
				case EActionType.AddVelocity: { return new AddVelocityActionModel(); }
				case EActionType.Block: { return new BlockActionModel(); }
				case EActionType.CameraShake: { return new CameraShakeActionModel(); }
				case EActionType.ConstantAcceleration: { return new ConstantAccelerationActionModel(); }
				case EActionType.ConstantDecceleration: { return new ConstantDeccelerationActionModel(); }
				case EActionType.CreateAttack: { return new CreateAttackActionModel(); }
				case EActionType.Shield: { return new ShieldActionModel(); }
				case EActionType.CreateHitCircle: { return new CreateHitCircleActionModel(); }
				case EActionType.CreateThrow: { return new CreateThrowActionModel(); }
				case EActionType.Deactivate: { return new DeactivateActionModel(); }
				case EActionType.Evade: { return new EvadeActionModel(); }
				case EActionType.KillPlayer: { return new KillPlayerActionModel(); }
				case EActionType.ParticleEffect: { return new ParticleEffectActionModel(); }
				case EActionType.PlayAnimation: { return new PlayAnimationActionModel(); }
				case EActionType.PlaySound: { return new PlaySoundActionModel(); }
				case EActionType.PointLight: { return new PointLightActionModel(); }
				case EActionType.Projectile: { return new ProjectileActionModel(); }
				case EActionType.Random: { return new RandomActionModel(); }
				case EActionType.Rotate: { return new RotateActionModel(); }
				case EActionType.SendStateMessage: { return new SendStateMessageActionModel(); }
				case EActionType.SendToBack: { return new SendToBackActionModel(); }
				case EActionType.SetVelocity: { return new SetVelocityActionModel(); }
				case EActionType.TargetRotation: { return new TargetRotationActionModel(); }
				case EActionType.Trail: { return new TrailActionModel(); }
				default: { throw new Exception($"unknown actionType: {stateActionType.ToString()}"); }
			}
		}

		private static EActionType XMLTypeToType(string actionType)
		{
			switch (actionType)
			{
				case "SPFSettings.AddVelocityActionXML": { return EActionType.AddVelocity; }
				case "SPFSettings.ConstantAccelerationActionXML": { return EActionType.ConstantAcceleration; }
				case "SPFSettings.ConstantDeccelerationActionXML": { return EActionType.ConstantDecceleration; }
				case "SPFSettings.CreateAttackActionXML": { return EActionType.CreateAttack; }
				case "SPFSettings.CreateBlockActionXML": { return EActionType.Shield; }
				case "SPFSettings.CreateThrowActionXML": { return EActionType.CreateThrow; }
				case "SPFSettings.DeactivateActionXML": { return EActionType.Deactivate; }
				case "SPFSettings.EvadeActionXML": { return EActionType.Evade; }
				case "SPFSettings.ParticleEffectActionXML": { return EActionType.ParticleEffect; }
				case "SPFSettings.PlayAnimationActionXML": { return EActionType.PlayAnimation; }
				case "SPFSettings.PlaySoundActionXML": { return EActionType.PlaySound; }
				case "SPFSettings.ProjectileActionXML": { return EActionType.Projectile; }
				case "SPFSettings.SendStateMessageActionXML": { return EActionType.SendStateMessage; }
				case "SPFSettings.SetVelocityActionXML": { return EActionType.SetVelocity; }
				case "SPFSettings.TrailActionXML": { return EActionType.Trail; }
				case "SPFSettings.AddGarmentActionXML": { return EActionType.AddGarment; }
				case "SPFSettings.BlockingStateActionXML": { return EActionType.Block; }
				case "SPFSettings.CreateHitCircleActionXML": { return EActionType.CreateHitCircle; }
				case "SPFSettings.RotateActionXML": { return EActionType.Rotate; }
				case "SPFSettings.TargetRotationActionXML": { return EActionType.TargetRotation; }
				case "SPFSettings.CameraShakeActionXML": { return EActionType.CameraShake; }
				case "SPFSettings.KillPlayerActionXML": { return EActionType.KillPlayer; }
				default: { throw new Exception($"unknown actionType: {actionType.ToString()}"); }
			}
		}

		public static BaseActionModel CreateActionModel(BaseAction actionModel)
		{
			switch (actionModel.ActionType)
			{
				case EActionType.AddGarment: { return new AddGarmentActionModel(actionModel); }
				case EActionType.AddVelocity: { return new AddVelocityActionModel(actionModel); }
				case EActionType.Block: { return new BlockActionModel(actionModel); }
				case EActionType.CameraShake: { return new CameraShakeActionModel(actionModel); }
				case EActionType.ConstantAcceleration: { return new ConstantAccelerationActionModel(actionModel); }
				case EActionType.ConstantDecceleration: { return new ConstantDeccelerationActionModel(actionModel); }
				case EActionType.CreateAttack: { return new CreateAttackActionModel(actionModel); }
				case EActionType.Shield: { return new ShieldActionModel(actionModel); }
				case EActionType.CreateHitCircle: { return new CreateHitCircleActionModel(actionModel); }
				case EActionType.CreateThrow: { return new CreateThrowActionModel(actionModel); }
				case EActionType.Deactivate: { return new DeactivateActionModel(actionModel); }
				case EActionType.Evade: { return new EvadeActionModel(actionModel); }
				case EActionType.KillPlayer: { return new KillPlayerActionModel(actionModel); }
				case EActionType.ParticleEffect: { return new ParticleEffectActionModel(actionModel); }
				case EActionType.PlayAnimation: { return new PlayAnimationActionModel(actionModel); }
				case EActionType.PlaySound: { return new PlaySoundActionModel(actionModel); }
				case EActionType.PointLight: { return new PointLightActionModel(actionModel); }
				case EActionType.Projectile: { return new ProjectileActionModel(actionModel); }
				case EActionType.Random: { return new RandomActionModel(actionModel); }
				case EActionType.Rotate: { return new RotateActionModel(actionModel); }
				case EActionType.SendStateMessage: { return new SendStateMessageActionModel(actionModel); }
				case EActionType.SendToBack: { return new SendToBackActionModel(actionModel); }
				case EActionType.SetVelocity: { return new SetVelocityActionModel(actionModel); }
				case EActionType.TargetRotation: { return new TargetRotationActionModel(actionModel); }
				case EActionType.Trail: { return new TrailActionModel(actionModel); }
				default: { throw new Exception($"unknown actionType: {actionModel.ActionType.ToString()}"); }
			}
		}

		/// <summary>
		/// factory method to create the correct action given an action type
		/// </summary>
		/// <param name="actionType">the type of action to add</param>
		/// <param name="owner">the owner of this action list</param>
		/// <returns>IBaseAction: reference to the action that was created</returns>
		public static BaseAction CreateStateAction(BaseActionModel actionModel, BaseObject owner, IStateContainer stateContainer)
		{
			//get the correct action type
			switch (actionModel.ActionType)
			{
				case EActionType.AddGarment: { return new AddGarmentAction(owner, actionModel); }
				case EActionType.AddVelocity: { return new AddVelocityAction(owner, actionModel); }
				case EActionType.Block: { return new BlockAction(owner, actionModel, stateContainer); }
				case EActionType.CameraShake: { return new CameraShakeAction(owner, actionModel); }
				case EActionType.ConstantAcceleration: { return new ConstantAccelerationAction(owner, actionModel); }
				case EActionType.ConstantDecceleration: { return new ConstantDeccelerationAction(owner, actionModel); }
				case EActionType.CreateAttack: { return new CreateAttackAction(owner, actionModel, stateContainer); }
				case EActionType.Shield: { return new ShieldAction(owner, actionModel, stateContainer); }
				case EActionType.CreateHitCircle: { return new CreateHitCircleAction(owner, actionModel, stateContainer); }
				case EActionType.CreateThrow: { return new CreateThrowAction(owner, actionModel, stateContainer); }
				case EActionType.Deactivate: { return new DeactivateAction(owner, actionModel); }
				case EActionType.Evade: { return new EvadeAction(owner, actionModel); }
				case EActionType.KillPlayer: { return new KillPlayerAction(owner, actionModel); }
				case EActionType.ParticleEffect: { return new ParticleEffectAction(owner, actionModel); }
				case EActionType.PlayAnimation: { return new PlayAnimationAction(owner, actionModel); }
				case EActionType.PlaySound: { return new PlaySoundAction(owner, actionModel); }
				case EActionType.PointLight: { return new PointLightAction(owner, actionModel); }
				case EActionType.Projectile: { return new ProjectileAction(owner, actionModel); }
				case EActionType.Random: { return new RandomAction(owner, actionModel, stateContainer); }
				case EActionType.Rotate: { return new RotateAction(owner, actionModel); }
				case EActionType.SendStateMessage: { return new SendStateMessageAction(owner, actionModel, stateContainer); }
				case EActionType.SendToBack: { return new SendToBackAction(owner, actionModel); }
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
				case EActionType.Block: { return new BlockAction(owner); }
				case EActionType.CameraShake: { return new CameraShakeAction(owner); }
				case EActionType.ConstantAcceleration: { return new ConstantAccelerationAction(owner); }
				case EActionType.ConstantDecceleration: { return new ConstantDeccelerationAction(owner); }
				case EActionType.CreateAttack: { return new CreateAttackAction(owner); }
				case EActionType.Shield: { return new ShieldAction(owner); }
				case EActionType.CreateHitCircle: { return new CreateHitCircleAction(owner); }
				case EActionType.CreateThrow: { return new CreateThrowAction(owner); }
				case EActionType.Deactivate: { return new DeactivateAction(owner); }
				case EActionType.Evade: { return new EvadeAction(owner); }
				case EActionType.KillPlayer: { return new KillPlayerAction(owner); }
				case EActionType.ParticleEffect: { return new ParticleEffectAction(owner); }
				case EActionType.PlayAnimation: { return new PlayAnimationAction(owner); }
				case EActionType.PlaySound: { return new PlaySoundAction(owner); }
				case EActionType.PointLight: { return new PointLightAction(owner); }
				case EActionType.Projectile: { return new ProjectileAction(owner); }
				case EActionType.Random: { return new RandomAction(owner); }
				case EActionType.Rotate: { return new RotateAction(owner); }
				case EActionType.SendStateMessage: { return new SendStateMessageAction(owner); }
				case EActionType.SendToBack: { return new SendToBackAction(owner); }
				case EActionType.SetVelocity: { return new SetVelocityAction(owner); }
				case EActionType.TargetRotation: { return new TargetRotationAction(owner); }
				case EActionType.Trail: { return new TrailAction(owner); }
				default: { throw new Exception($"unknown actionType: {actionType}"); }
			}
		}

		#endregion //Methods
	}
}