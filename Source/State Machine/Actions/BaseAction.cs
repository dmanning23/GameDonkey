using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;

namespace GameDonkey
{
	public enum EActionType
	{
		AddGarment = 0,
		PlayAnimation,
		AddVelocity,
		SetVelocity,
		ConstantAcceleration,
		ConstantDecceleration,
		CreateBlock,
		Evade,
		Projectile,
		PlaySound,
		Trail,
		CreateAttack,
		BlockState,
		CreateHitCircle,
		CreateThrow,
		ParticleEffect,
		SendStateMessage,
		Deactivate,
		NumTypes
	}

	/// <summary>
	/// class for sorting actions in a 
	/// </summary>
	class ActionSort : IComparer<IBaseAction>
	{
		public int Compare(IBaseAction action1, IBaseAction action2)
		{
			if (action1.Time != action2.Time)
			{
				return action1.Time.CompareTo(action2.Time);
			}
			else
			{
				return action1.ActionType.CompareTo(action2.ActionType);
			}
		}
	}

	/// <summary>
	/// The base interface for state machine actions
	/// </summary>
	public abstract class IBaseAction
	{
		#region Members

		/// <summary>
		/// the type of this action
		/// </summary>
		private EActionType m_eActionType;

		/// <summary>
		/// the time from the start of the state that this action ocuurs
		/// </summary>
		private float m_fTime;

		/// <summary>
		/// whether or not this action has been run 
		/// </summary>
		private bool m_bAlreadyRun;

		/// <summary>
		/// The game object that owns this action
		/// </summary>
		private BaseObject m_rOwner;

		#endregion //Members

		#region Properties

		public EActionType ActionType
		{
			get { return m_eActionType; }
			set { m_eActionType = value; }
		}

		public BaseObject Owner
		{
			get { return m_rOwner; }
			set { m_rOwner = value; }
		}

		public bool AlreadyRun
		{
			get { return m_bAlreadyRun; }
			set { m_bAlreadyRun = value; }
		}

		public float Time
		{
			get { return m_fTime; }
			set { m_fTime = value; }
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// default constructor
		/// </summary>
		public IBaseAction(BaseObject rOwner)
		{
			m_eActionType = EActionType.NumTypes;
			m_fTime = 0.0f;
			m_bAlreadyRun = true;
			m_rOwner = rOwner;
		}

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public virtual bool Execute()
		{
			m_bAlreadyRun = true;
			return false;
		}

		public static EActionType StringToType(string strType)
		{
			for (EActionType i = 0; i < EActionType.NumTypes; i++)
			{
				if (strType == i.ToString())
				{
					return i;
				}
			}
			Debug.Assert(false);
			return EActionType.NumTypes;
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

		public override string ToString()
		{
			return m_fTime.ToString() + ": " + TypeToXMLString(m_eActionType);
		}

		public abstract bool Compare(IBaseAction rInst);

		#endregion //Methods

		#region File IO

		static public bool ReadListActions(BaseObject rOwner,
			ref List<IBaseAction> outputList,
			XmlNode rParentNode,
			IGameDonkey rEngine,
			StateMachine rStateMachine)
		{
			//set up all the actions
			if (!rParentNode.HasChildNodes)
			{
				return true;
			}

			for (XmlNode childNode = rParentNode.FirstChild;
				null != childNode;
				childNode = childNode.NextSibling)
			{
				if ("Item" != childNode.Name)
				{
					return false;
				}

				//should have an attribute Type
				EActionType eChildType = EActionType.NumTypes;
				XmlNamedNodeMap mapAttributes = childNode.Attributes;
				for (int i = 0; i < mapAttributes.Count; i++)
				{
					//will only have the name attribute
					string strName = mapAttributes.Item(i).Name;
					string strValue = mapAttributes.Item(i).Value;
					if ("Type" == strName)
					{
						eChildType = IBaseAction.XMLTypeToType(strValue);
					}
					else
					{
						Debug.Assert(false);
						return false;
					}
				}

				switch (eChildType)
				{
					case EActionType.AddVelocity:
					{
						CAddVelocityAction myAction = new CAddVelocityAction(rOwner);
						if (!myAction.ReadSerialized(childNode, rEngine))
						{
							Debug.Assert(false);
							return false;
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.ConstantAcceleration:
					{
						CConstantAccelerationAction myAction = new CConstantAccelerationAction(rOwner);
						if (!myAction.ReadSerialized(childNode, rEngine))
						{
							Debug.Assert(false);
							return false;
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.ConstantDecceleration:
					{
						CConstantDeccelerationAction myAction = new CConstantDeccelerationAction(rOwner);
						if (!myAction.ReadSerialized(childNode, rEngine))
						{
							Debug.Assert(false);
							return false;
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.CreateAttack:
					{
						CCreateAttackAction myAction = new CCreateAttackAction(rOwner);
						if (!myAction.ReadSerialized(childNode, rEngine, rStateMachine))
						{
							Debug.Assert(false);
							return false;
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.CreateBlock:
					{
						CCreateBlockAction myAction = new CCreateBlockAction(rOwner);
						if (!myAction.ReadSerialized(childNode, rEngine))
						{
							Debug.Assert(false);
							return false;
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.CreateThrow:
					{
						CCreateThrowAction myAction = new CCreateThrowAction(rOwner);
						if (!myAction.ReadSerialized(childNode, rEngine, rStateMachine))
						{
							Debug.Assert(false);
							return false;
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.Deactivate:
					{
						CDeactivateAction myAction = new CDeactivateAction(rOwner);
						if (!myAction.ReadSerialized(childNode, rEngine))
						{
							Debug.Assert(false);
							return false;
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.Evade:
					{
						CEvadeAction myAction = new CEvadeAction(rOwner);
						if (!myAction.ReadSerialized(childNode, rEngine))
						{
							Debug.Assert(false);
							return false;
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.ParticleEffect:
					{
						CParticleEffectAction myAction = new CParticleEffectAction(rOwner, rEngine);
						if (!myAction.ReadSerialized(childNode, rEngine))
						{
							Debug.Assert(false);
							return false;
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.PlayAnimation:
					{
						CPlayAnimationAction myAction = new CPlayAnimationAction(rOwner);
						if (!myAction.ReadSerialized(childNode, rEngine))
						{
							Debug.Assert(false);
							return false;
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.PlaySound:
					{
						CPlaySoundAction myAction = new CPlaySoundAction(rOwner, rEngine);
						if (!myAction.ReadSerialized(childNode))
						{
							Debug.Assert(false);
							return false;
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.Projectile:
					{
						CProjectileAction myAction = new CProjectileAction(rOwner);
						if (!myAction.ReadSerialized(childNode, rEngine))
						{
							Debug.Assert(false);
							return false;
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.SendStateMessage:
					{
						CSendStateMessageAction myAction = new CSendStateMessageAction(rOwner);
						if (!myAction.ReadSerialized(childNode, rEngine))
						{
							Debug.Assert(false);
							return false;
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.SetVelocity:
					{
						CSetVelocityAction myAction = new CSetVelocityAction(rOwner);
						if (!myAction.ReadSerialized(childNode, rEngine))
						{
							Debug.Assert(false);
							return false;
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.Trail:
					{
						CTrailAction myAction = new CTrailAction(rOwner);
						if (!myAction.ReadSerialized(childNode, rEngine))
						{
							Debug.Assert(false);
							return false;
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.AddGarment:
					{
						AddGarmentAction myAction = new AddGarmentAction(rOwner);
						if (!myAction.ReadSerialized(childNode, rEngine))
						{
							Debug.Assert(false);
							return false;
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.BlockState:
					{
						BlockingStateAction myAction = new BlockingStateAction(rOwner);
						if (!myAction.ReadSerialized(childNode, rEngine, rStateMachine))
						{
							Debug.Assert(false);
							return false;
						}
						outputList.Add(myAction);
					}
					break;
					default:
					{
						Debug.Assert(false);
						return false;
					}
				}
			}

			return true;
		}

		public void WriteXMLFormat(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("Item");
			rXMLFile.WriteAttributeString("Type", TypeToXMLString(m_eActionType));

			//write out the type
			rXMLFile.WriteStartElement("type");
			rXMLFile.WriteString(ActionType.ToString());
			rXMLFile.WriteEndElement();

			//write out the time
			rXMLFile.WriteStartElement("time");
			rXMLFile.WriteString(m_fTime.ToString());
			rXMLFile.WriteEndElement();

			//write out the action specific crap
			WriteXML(rXMLFile);

			rXMLFile.WriteEndElement(); //Item
		}

		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		public abstract void WriteXML(XmlTextWriter rXMLFile);

		static public void ReadListActions(BaseObject rOwner,
			List<SPFSettings.BaseActionXML> inputList,
			ref List<IBaseAction> outputList,
			IGameDonkey rEngine,
			ContentManager rXmlContent,
			StateMachine rStateMachine)
		{
			Debug.Assert(null != rOwner);

			//set up all the actions
			for (int i = 0; i < inputList.Count; i++)
			{
#if DEBUG
				//if this isn't the first item, make sure the time is greater than the previous item
				if (i > 0)
				{
					Debug.Assert(inputList[i - 1].time <= inputList[i].time);
				}
#endif
				//what type of action is it?
				EActionType eType = IBaseAction.StringToType(inputList[i].type);

				switch (eType)
				{
					case EActionType.AddVelocity:
					{
						CAddVelocityAction myAction = new CAddVelocityAction(rOwner);
						SPFSettings.AddVelocityActionXML myActionXML = (SPFSettings.AddVelocityActionXML)inputList[i];
						if (myAction.ReadSerialized(myActionXML))
						{
							outputList.Add(myAction);
						}
						else
						{
							//some unknown error occurred? better look into that!
							Debug.Assert(false);
						}
					}
					break;
					case EActionType.ConstantAcceleration:
					{
						CConstantAccelerationAction myAction = new CConstantAccelerationAction(rOwner);
						SPFSettings.ConstantAccelerationActionXML myActionXML = (SPFSettings.ConstantAccelerationActionXML)inputList[i];
						if (myAction.ReadSerialized(myActionXML))
						{
							outputList.Add(myAction);
						}
						else
						{
							//some unknown error occurred? better look into that!
							Debug.Assert(false);
						}
					}
					break;
					case EActionType.ConstantDecceleration:
					{
						CConstantDeccelerationAction myAction = new CConstantDeccelerationAction(rOwner);
						SPFSettings.ConstantDeccelerationActionXML myActionXML = (SPFSettings.ConstantDeccelerationActionXML)inputList[i];
						if (myAction.ReadSerialized(myActionXML))
						{
							outputList.Add(myAction);
						}
						else
						{
							//some unknown error occurred? better look into that!
							Debug.Assert(false);
						}
					}
					break;
					case EActionType.CreateAttack:
					{
						CCreateAttackAction myAction = new CCreateAttackAction(rOwner);
						SPFSettings.CreateAttackActionXML myActionXML = (SPFSettings.CreateAttackActionXML)inputList[i];
						if (myAction.ReadSerialized(myActionXML, rEngine, rXmlContent, rStateMachine))
						{
							outputList.Add(myAction);
						}
						else
						{
							//some unknown error occurred? better look into that!
							Debug.Assert(false);
						}
					}
					break;
					case EActionType.CreateBlock:
					{
						CCreateBlockAction myAction = new CCreateBlockAction(rOwner);
						SPFSettings.CreateBlockActionXML myActionXML = (SPFSettings.CreateBlockActionXML)inputList[i];
						if (!myAction.ReadSerialized(myActionXML))
						{
							Debug.Assert(false);
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.CreateThrow:
					{
						CCreateThrowAction myAction = new CCreateThrowAction(rOwner);
						SPFSettings.CreateThrowActionXML myActionXML = (SPFSettings.CreateThrowActionXML)inputList[i];
						if (myAction.ReadSerialized(myActionXML, rEngine, rXmlContent, rStateMachine))
						{
							outputList.Add(myAction);
						}
						else
						{
							//some unknown error occurred? better look into that!
							Debug.Assert(false);
						}
					}
					break;
					case EActionType.Deactivate:
					{
						CDeactivateAction myAction = new CDeactivateAction(rOwner);
						SPFSettings.DeactivateActionXML myActionXML = (SPFSettings.DeactivateActionXML)inputList[i];
						if (!myAction.ReadSerialized(myActionXML))
						{
							Debug.Assert(false);
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.Evade:
					{
						CEvadeAction myAction = new CEvadeAction(rOwner);
						SPFSettings.EvadeActionXML myActionXML = (SPFSettings.EvadeActionXML)inputList[i];
						if (!myAction.ReadSerialized(myActionXML))
						{
							Debug.Assert(false);
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.ParticleEffect:
					{
						CParticleEffectAction myAction = new CParticleEffectAction(rOwner, rEngine);
						SPFSettings.ParticleEffectActionXML myActionXML = (SPFSettings.ParticleEffectActionXML)inputList[i];
						if (!myAction.ReadSerialized(myActionXML, rEngine))
						{
							Debug.Assert(false);
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.PlayAnimation:
					{
						CPlayAnimationAction myAction = new CPlayAnimationAction(rOwner);
						SPFSettings.PlayAnimationActionXML myActionXML = (SPFSettings.PlayAnimationActionXML)inputList[i];
						if (!myAction.ReadSerialized(myActionXML))
						{
							Debug.Assert(false);
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.PlaySound:
					{
						CPlaySoundAction myAction = new CPlaySoundAction(rOwner, rEngine);
						SPFSettings.PlaySoundActionXML myActionXML = (SPFSettings.PlaySoundActionXML)inputList[i];
						if (!myAction.ReadSerialized(myActionXML))
						{
							Debug.Assert(false);
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.Projectile:
					{
						CProjectileAction myAction = new CProjectileAction(rOwner);
						SPFSettings.ProjectileActionXML myActionXML = (SPFSettings.ProjectileActionXML)inputList[i];
						if (!myAction.ReadSerialized(myActionXML, rEngine, rXmlContent))
						{
							Debug.Assert(false);
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.SendStateMessage:
					{
						CSendStateMessageAction myAction = new CSendStateMessageAction(rOwner);
						SPFSettings.SendStateMessageActionXML myActionXML = (SPFSettings.SendStateMessageActionXML)inputList[i];
						if (!myAction.ReadSerialized(myActionXML, rStateMachine))
						{
							Debug.Assert(false);
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.SetVelocity:
					{
						CSetVelocityAction myAction = new CSetVelocityAction(rOwner);
						SPFSettings.SetVelocityActionXML myActionXML = (SPFSettings.SetVelocityActionXML)inputList[i];
						if (!myAction.ReadSerialized(myActionXML))
						{
							Debug.Assert(false);
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.Trail:
					{
						CTrailAction myAction = new CTrailAction(rOwner);
						SPFSettings.TrailActionXML myActionXML = (SPFSettings.TrailActionXML)inputList[i];
						if (!myAction.ReadSerialized(myActionXML))
						{
							Debug.Assert(false);
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.AddGarment:
					{
						AddGarmentAction myAction = new AddGarmentAction(rOwner);
						SPFSettings.AddGarmentActionXML myActionXML = (SPFSettings.AddGarmentActionXML)inputList[i];
						if (!myAction.ReadSerialized(rXmlContent, myActionXML, rEngine))
						{
							Debug.Assert(false);
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.BlockState:
					{
						BlockingStateAction myAction = new BlockingStateAction(rOwner);
						SPFSettings.BlockingStateActionXML myActionXML = (SPFSettings.BlockingStateActionXML)inputList[i];
						if (!myAction.ReadSerialized(myActionXML, rEngine, rXmlContent, rStateMachine))
						{
							Debug.Assert(false);
						}
						outputList.Add(myAction);
					}
					break;
					default:
					{
						Debug.Assert(false);
					}
					break;
				}
			}
		}

		/// <summary>
		/// read in base action from serialized file
		/// </summary>
		/// <param name="myAction">the dude to read from</param>
		public void ReadSerializedBase(SPFSettings.BaseActionXML myAction)
		{
			m_fTime = myAction.time;
		}

		#endregion //File IO
	}
}
