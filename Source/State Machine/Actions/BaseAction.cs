using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace GameDonkey
{
	/// <summary>
	/// The base interface for state machine actions
	/// </summary>
	public abstract class IBaseAction
	{
		#region Properties

		/// <summary>
		/// the type of this action
		/// </summary>
		public EActionType ActionType { get; protected set; }

		/// <summary>
		/// The game object that owns this action
		/// </summary>
		public BaseObject Owner { get; set; }

		/// <summary>
		/// whether or not this action has been run 
		/// </summary>
		public bool AlreadyRun { get; set; }

		/// <summary>
		/// the time from the start of the state that this action ocuurs
		/// </summary>
		public float Time { get; set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// default constructor
		/// </summary>
		public IBaseAction(BaseObject rOwner)
		{
			ActionType = EActionType.NumTypes;
			Time = 0.0f;
			AlreadyRun = true;
			Owner = rOwner;
		}

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
			return Time.ToString() + ": " + StateActionFactory.TypeToXMLString(ActionType);
		}

		public abstract bool Compare(IBaseAction rInst);

		#endregion //Methods

		#region File IO

		/// <summary>
		/// Read from an xml file
		/// </summary>
		/// <param name="rXMLNode">the xml node to read from</param>
		/// <returns></returns>
		public abstract bool ReadXml(XmlNode rXMLNode, IGameDonkey rEngine, SingleStateContainer stateContainer);

		static public bool ReadXmlListActions(BaseObject rOwner,
			ref List<IBaseAction> outputList,
			XmlNode rParentNode,
			IGameDonkey rEngine,
			SingleStateContainer stateContainer)
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
						eChildType = StateActionFactory.XMLTypeToType(strValue);
					}
					else
					{
						Debug.Assert(false);
						return false;
					}
				}

				IBaseAction myAction = StateActionFactory.CreateStateAction(eChildType, rOwner, rEngine);
				if (!myAction.ReadXml(childNode, rEngine, stateContainer))
				{
					Debug.Assert(false);
					return false;
				}
				outputList.Add(myAction);
			}

			return true;
		}

		/// <summary>
		/// Write out all the xml required for a state action
		/// </summary>
		/// <param name="rXMLFile"></param>
		public void WriteXml(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("Item");
			rXMLFile.WriteAttributeString("Type", StateActionFactory.TypeToXMLString(ActionType));

			//write out the type
			rXMLFile.WriteStartElement("type");
			rXMLFile.WriteString(ActionType.ToString());
			rXMLFile.WriteEndElement();

			//write out the time
			rXMLFile.WriteStartElement("time");
			rXMLFile.WriteString(Time.ToString());
			rXMLFile.WriteEndElement();

			//write out the action specific crap
			WriteActionXml(rXMLFile);

			rXMLFile.WriteEndElement(); //Item
		}

		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		protected virtual void WriteActionXml(XmlTextWriter rXMLFile)
		{
		}

		static public void ReadSerializedListActions(BaseObject rOwner,
			List<SPFSettings.BaseActionXML> inputList,
			ref List<IBaseAction> outputList,
			IGameDonkey rEngine,
			ContentManager rXmlContent,
			SingleStateContainer stateContainer)
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
				EActionType eType = (EActionType)Enum.Parse(typeof(EActionType), inputList[i].type);

				switch (eType)
				{
					case EActionType.AddVelocity:
					{
						AddVelocityAction myAction = new AddVelocityAction(rOwner);
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
						ConstantAccelerationAction myAction = new ConstantAccelerationAction(rOwner);
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
						ConstantDeccelerationAction myAction = new ConstantDeccelerationAction(rOwner);
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
						CreateAttackAction myAction = new CreateAttackAction(rOwner);
						SPFSettings.CreateAttackActionXML myActionXML = (SPFSettings.CreateAttackActionXML)inputList[i];
						if (myAction.ReadSerialized(myActionXML, rEngine, rXmlContent, stateContainer))
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
						CreateBlockAction myAction = new CreateBlockAction(rOwner);
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
						CreateThrowAction myAction = new CreateThrowAction(rOwner);
						SPFSettings.CreateThrowActionXML myActionXML = (SPFSettings.CreateThrowActionXML)inputList[i];
						if (myAction.ReadSerialized(myActionXML, rEngine, rXmlContent, stateContainer))
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
						DeactivateAction myAction = new DeactivateAction(rOwner);
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
						EvadeAction myAction = new EvadeAction(rOwner);
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
						ParticleEffectAction myAction = new ParticleEffectAction(rOwner, rEngine);
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
						PlayAnimationAction myAction = new PlayAnimationAction(rOwner);
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
						PlaySoundAction myAction = new PlaySoundAction(rOwner, rEngine);
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
						ProjectileAction myAction = new ProjectileAction(rOwner);
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
						SendStateMessageAction myAction = new SendStateMessageAction(rOwner);
						SPFSettings.SendStateMessageActionXML myActionXML = (SPFSettings.SendStateMessageActionXML)inputList[i];
						if (!myAction.ReadSerialized(myActionXML, stateContainer))
						{
							Debug.Assert(false);
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.SetVelocity:
					{
						SetVelocityAction myAction = new SetVelocityAction(rOwner);
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
						TrailAction myAction = new TrailAction(rOwner);
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
						if (!myAction.ReadSerialized(myActionXML, rEngine, rXmlContent, stateContainer))
						{
							Debug.Assert(false);
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.Rotate:
					{
						RotateAction myAction = new RotateAction(rOwner);
						SPFSettings.RotateActionXML myActionXML = (SPFSettings.RotateActionXML)inputList[i];
						if (!myAction.ReadSerialized(myActionXML))
						{
							Debug.Assert(false);
						}
						outputList.Add(myAction);
					}
					break;
					case EActionType.TargetRotation:
					{
						TargetRotationAction myAction = new TargetRotationAction(rOwner);
						SPFSettings.TargetRotationActionXML myActionXML = (SPFSettings.TargetRotationActionXML)inputList[i];
						if (!myAction.ReadSerialized(myActionXML))
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
			Time = myAction.time;
		}

		#endregion //File IO
	}
}
