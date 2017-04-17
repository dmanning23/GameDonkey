using System.Diagnostics;

namespace GameDonkeyLib
{
	/// <summary>
	/// this thing does a lot of sorting between different enums and state action objects, etc.
	/// </summary>
	public static class StateActionFactory
	{
		#region Methods

		/// <summary>
		/// factory method to create the correct action given an action type
		/// </summary>
		/// <param name="eType">the type of action to add</param>
		/// <param name="rOwner">the owner of this action list</param>
		/// <returns>IBaseAction: reference to the action that was created</returns>
		public static BaseAction CreateStateAction(EActionType eType, BaseObject rOwner, IGameDonkey rEngine)
		{
			//get the correct action type
			switch (eType)
			{
				case EActionType.AddGarment: { return new AddGarmentAction(rOwner); }
				case EActionType.PlayAnimation: { return new PlayAnimationAction(rOwner); }
				case EActionType.AddVelocity: { return new AddVelocityAction(rOwner); }
				case EActionType.SetVelocity: { return new SetVelocityAction(rOwner); }
				case EActionType.ConstantAcceleration: { return new ConstantAccelerationAction(rOwner); }
				case EActionType.ConstantDecceleration: { return new ConstantDeccelerationAction(rOwner); }
				case EActionType.CreateBlock: { return new CreateBlockAction(rOwner); }
				case EActionType.Evade: { return new EvadeAction(rOwner); }
				case EActionType.Projectile: { return new ProjectileAction(rOwner); }
				case EActionType.PlaySound: { return new PlaySoundAction(rOwner, rEngine); }
				case EActionType.Trail: { return new TrailAction(rOwner); }
				case EActionType.CreateAttack: { return new CreateAttackAction(rOwner); }
				case EActionType.BlockState: { return new BlockingStateAction(rOwner); }
				case EActionType.CreateHitCircle: { return new CreateHitCircleAction(rOwner); }
				case EActionType.CreateThrow: { return new CreateThrowAction(rOwner); }
				case EActionType.ParticleEffect: { return new ParticleEffectAction(rOwner, rEngine); }
				case EActionType.SendStateMessage: { return new SendStateMessageAction(rOwner); }
				case EActionType.Deactivate: { return new DeactivateAction(rOwner); }
				case EActionType.Rotate: { return new RotateAction(rOwner); }
				case EActionType.TargetRotation: { return new TargetRotationAction(rOwner); }
				case EActionType.CameraShake: { return new CameraShakeAction(rOwner); }
				case EActionType.KillPlayer: { return new KillPlayerAction(rOwner); }
				default: { Debug.Assert(false); return null; }
			}
		}

		public static EActionType XMLTypeToType(string strXMLType)
		{
			switch (strXMLType)
			{
				case "SPFSettings.AddVelocityActionXML": { return EActionType.AddVelocity; }
				case "SPFSettings.ConstantAccelerationActionXML": { return EActionType.ConstantAcceleration; }
				case "SPFSettings.ConstantDeccelerationActionXML": { return EActionType.ConstantDecceleration; }
				case "SPFSettings.CreateAttackActionXML": { return EActionType.CreateAttack; }
				case "SPFSettings.CreateBlockActionXML": { return EActionType.CreateBlock; }
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
				case "SPFSettings.BlockingStateActionXML": { return EActionType.BlockState; }
				case "SPFSettings.CreateHitCircleActionXML": { return EActionType.CreateHitCircle; }
				case "SPFSettings.RotateActionXML": { return EActionType.Rotate; }
				case "SPFSettings.TargetRotationActionXML": { return EActionType.TargetRotation; }
				case "SPFSettings.CameraShakeActionXML": { return EActionType.CameraShake; }
				case "SPFSettings.KillPlayerActionXML": { return EActionType.KillPlayer; }
				default: { Debug.Assert(false); return EActionType.NumTypes; }
			}
		}

		public static string TypeToXMLString(EActionType eType)
		{
			switch (eType)
			{
				case EActionType.AddVelocity: { return "SPFSettings.AddVelocityActionXML"; }
				case EActionType.ConstantAcceleration: { return "SPFSettings.ConstantAccelerationActionXML"; }
				case EActionType.ConstantDecceleration: { return "SPFSettings.ConstantDeccelerationActionXML"; }
				case EActionType.CreateAttack: { return "SPFSettings.CreateAttackActionXML"; }
				case EActionType.CreateBlock: { return "SPFSettings.CreateBlockActionXML"; }
				case EActionType.CreateHitCircle: { return "SPFSettings.CreateHitCircleActionXML"; }
				case EActionType.CreateThrow: { return "SPFSettings.CreateThrowActionXML"; }
				case EActionType.Deactivate: { return "SPFSettings.DeactivateActionXML"; }
				case EActionType.Evade: { return "SPFSettings.EvadeActionXML"; }
				case EActionType.ParticleEffect: { return "SPFSettings.ParticleEffectActionXML"; }
				case EActionType.PlayAnimation: { return "SPFSettings.PlayAnimationActionXML"; }
				case EActionType.PlaySound: { return "SPFSettings.PlaySoundActionXML"; }
				case EActionType.Projectile: { return "SPFSettings.ProjectileActionXML"; }
				case EActionType.SendStateMessage: { return "SPFSettings.SendStateMessageActionXML"; }
				case EActionType.SetVelocity: { return "SPFSettings.SetVelocityActionXML"; }
				case EActionType.Trail: { return "SPFSettings.TrailActionXML"; }
				case EActionType.AddGarment: { return "SPFSettings.AddGarmentActionXML"; }
				case EActionType.BlockState: { return "SPFSettings.BlockingStateActionXML"; }
				case EActionType.Rotate: { return "SPFSettings.RotateActionXML"; }
				case EActionType.TargetRotation: { return "SPFSettings.TargetRotationActionXML"; }
				case EActionType.CameraShake: { return "SPFSettings.CameraShakeActionXML"; }
				case EActionType.KillPlayer: { return "SPFSettings.KillPlayerActionXML"; }
				default: { Debug.Assert(false); return ""; }
			}
		}

		#endregion //Methods
	}
}