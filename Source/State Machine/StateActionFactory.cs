using System.Diagnostics;

namespace GameDonkey
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
		public static IBaseAction CreateStateAction(EActionType eType, BaseObject rOwner, IGameDonkey rEngine)
		{
			//get the correct action type
			switch (eType)
			{
				case EActionType.AddGarment: { return new AddGarmentAction(rOwner); }
				case EActionType.AddVelocity: { return new AddVelocityAction(rOwner); }
				case EActionType.ConstantAcceleration: { return new ConstantAccelerationAction(rOwner); }
				case EActionType.ConstantDecceleration: { return new ConstantDeccelerationAction(rOwner); }
				case EActionType.CreateAttack: { return new CreateAttackAction(rOwner); }
				case EActionType.CreateBlock: { return new CreateBlockAction(rOwner); }
				case EActionType.CreateThrow: { return new CreateThrowAction(rOwner); }
				case EActionType.Deactivate: { return new DeactivateAction(rOwner); }
				case EActionType.Evade: { return new EvadeAction(rOwner); }
				case EActionType.ParticleEffect: { return new ParticleEffectAction(rOwner, rEngine); }
				case EActionType.PlayAnimation: { return new PlayAnimationAction(rOwner); }
				case EActionType.PlaySound: { return new PlaySoundAction(rOwner, rEngine); }
				case EActionType.Projectile: { return new ProjectileAction(rOwner); }
				case EActionType.SendStateMessage: { return new SendStateMessageAction(rOwner); }
				case EActionType.SetVelocity: { return new SetVelocityAction(rOwner); }
				case EActionType.Trail: { return new TrailAction(rOwner); }
				case EActionType.BlockState: { return new BlockingStateAction(rOwner); }
				default: { Debug.Assert(false); return null; }
			}
		}

		public static EActionType TroikaStringToType(string strType)
		{
			if (strType == "CreateAttack") { return EActionType.CreateAttack; }
			else if (strType == "CreateBlock") { return EActionType.CreateBlock; }
			else if (strType == "CreateThrow") { return EActionType.CreateThrow; }
			else if (strType == "Deactivate") { return EActionType.Deactivate; }
			else if (strType == "Evade") { return EActionType.Evade; }
			else if (strType == "CreateParticleEffect") { return EActionType.ParticleEffect; }
			else if (strType == "PlayAnimation") { return EActionType.PlayAnimation; }
			else if (strType == "PlaySound") { return EActionType.PlaySound; }
			else if (strType == "AddProjectile") { return EActionType.Projectile; }
			else if (strType == "SendStateMessage") { return EActionType.SendStateMessage; }
			else if (strType == "AddVelocity") { return EActionType.SetVelocity; }
			else if (strType == "Trail") { return EActionType.Trail; }
			else { Debug.Assert(false); return EActionType.NumTypes; }
		}

		public static EActionType XMLTypeToType(string strXMLType)
		{
			if (strXMLType == "SPFSettings.AddVelocityActionXML") { return EActionType.AddVelocity; }
			else if (strXMLType == "SPFSettings.ConstantAccelerationActionXML") { return EActionType.ConstantAcceleration; }
			else if (strXMLType == "SPFSettings.ConstantDeccelerationActionXML") { return EActionType.ConstantDecceleration; }
			else if (strXMLType == "SPFSettings.CreateAttackActionXML") { return EActionType.CreateAttack; }
			else if (strXMLType == "SPFSettings.CreateBlockActionXML") { return EActionType.CreateBlock; }
			else if (strXMLType == "SPFSettings.CreateThrowActionXML") { return EActionType.CreateThrow; }
			else if (strXMLType == "SPFSettings.DeactivateActionXML") { return EActionType.Deactivate; }
			else if (strXMLType == "SPFSettings.EvadeActionXML") { return EActionType.Evade; }
			else if (strXMLType == "SPFSettings.ParticleEffectActionXML") { return EActionType.ParticleEffect; }
			else if (strXMLType == "SPFSettings.PlayAnimationActionXML") { return EActionType.PlayAnimation; }
			else if (strXMLType == "SPFSettings.PlaySoundActionXML") { return EActionType.PlaySound; }
			else if (strXMLType == "SPFSettings.ProjectileActionXML") { return EActionType.Projectile; }
			else if (strXMLType == "SPFSettings.SendStateMessageActionXML") { return EActionType.SendStateMessage; }
			else if (strXMLType == "SPFSettings.SetVelocityActionXML") { return EActionType.SetVelocity; }
			else if (strXMLType == "SPFSettings.TrailActionXML") { return EActionType.Trail; }
			else if (strXMLType == "SPFSettings.AddGarmentActionXML") { return EActionType.AddGarment; }
			else if (strXMLType == "SPFSettings.BlockingStateActionXML") { return EActionType.BlockState; }
			else { Debug.Assert(false); return EActionType.NumTypes; }
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
				default: { Debug.Assert(false); return ""; }
			}
		}

		#endregion //Methods
	}
}