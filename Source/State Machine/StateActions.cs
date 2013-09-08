using System.Collections.Generic;
using GameTimer;
using System.Diagnostics;
using System.Xml;
using Microsoft.Xna.Framework.Content;
using StateMachineBuddy;

namespace GameDonkey
{
	/// <summary>
	/// A list of actions to perform while in one state
	/// </summary>
	public class CStateActions
	{
		#region Members

		/// <summary>
		/// List of all the actions to perform when in this state
		/// </summary>
		protected List<IBaseAction> m_listActions;

		/// <summary>
		/// name of the state this thing is describing
		/// </summary>
		protected string m_strStateName;

		/// <summary>
		/// whether or not this is an attack/throw state
		/// </summary>
		protected bool m_bIsAttack;

		/// <summary>
		/// If this state is an attack or a throw, the time that the startup ends
		/// </summary>
		protected float m_fActiveTime;

		/// <summary>
		/// The time that this action enters the "recovery" phase and can be cancelled into other actions
		/// </summary>
		protected float m_fRecoveryTime;

		#endregion //Members

		#region Properties

		public string Name
		{
			get { return m_strStateName; }
		}

		public bool IsAttack
		{
			get { return m_bIsAttack; }
		}

		public List<IBaseAction> Actions
		{
			get { return m_listActions; }
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// standard constructor!
		/// </summary>
		public CStateActions()
		{
			m_listActions = new List<IBaseAction>();
		}

		/// <summary>
		/// The states have changed, go through and set all the actions of the new state to "not run"
		/// </summary>
		public void StateChange()
		{
			for (int i = 0; i < m_listActions.Count; i++ )
			{
				m_listActions[i].AlreadyRun = false;
			}
		}

		/// <summary>
		/// Execute the actions that occur between the time slice. 
		/// Time is measured in seconds since entering the state
		/// </summary>
		/// <param name="fPrevTime">time of the previous frame</param>
		/// <param name="fCurTime">the current time</param>
		public void ExecuteAction(float fPrevTime, float fCurTime)
		{
			Debug.Assert(null != m_listActions);

			//loop through all actions, execute the ones between the time slice
			for (int i = 0; i < m_listActions.Count; i++)
			{
				//first check if the time of this action is expired
				if (m_listActions[i].Time > fCurTime)
				{
					//this action hasn't been run yet!
					return;
				}

				//check if this action hasn't happened yet
				else if (m_listActions[i].Time >= fPrevTime)
				{
					//if the action is expired, make sure it has been run anyways
					if (!m_listActions[i].AlreadyRun)
					{
						if (m_listActions[i].Execute())
						{
							//the state was changed when that dude was running
							return;
						}
					}
				}
			}

#if DEBUG
			////okay, if it gets here and the last action is a deactivate/sendstatemessage, 
			////and its already been done, it means something broke somewhere
			//IBaseAction rLastAction = m_listActions[m_listActions.Count - 1];
			//if (rLastAction.AlreadyRun && 
			//    ((rLastAction.ActionType == EActionType.Deactivate) || 
			//    (rLastAction.ActionType == EActionType.SendStateMessage)))
			//{
			//    Debug.Assert(false);
			//}
#endif
		}

		public bool Compare(CStateActions rInst)
		{
			if (m_strStateName != rInst.m_strStateName)
			{
				Debug.Assert(false);
				return false;
			}
			if (m_fActiveTime != rInst.m_fActiveTime)
			{
				Debug.Assert(false);
				return false;
			}
			if (m_fRecoveryTime != rInst.m_fRecoveryTime)
			{
				Debug.Assert(false);
				return false;
			}
			if (m_listActions.Count != rInst.m_listActions.Count)
			{
				Debug.Assert(false);
				return false;
			}

			//compare all the actions
			for (int i = 0; i < m_listActions.Count; i++)
			{
				if (!m_listActions[i].Compare(rInst.m_listActions[i]))
				{
					Debug.Assert(false);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Calculate the startup and recovery times for this state
		/// </summary>
		private void CalculateAttackTime()
		{
			//does this state have any attack actions?
			for (int i = 0; i < m_listActions.Count; i++)
			{
				if ((m_listActions[i].ActionType == EActionType.CreateAttack) ||
					(m_listActions[i].ActionType == EActionType.CreateThrow) ||
					(m_listActions[i].ActionType == EActionType.CreateHitCircle))
				{
					//set this state to an attack state
					m_bIsAttack = true;

					//check if this is the end of the startup 
					CCreateAttackAction rAction = (CCreateAttackAction)m_listActions[i];
					if (0.0f == m_fActiveTime)
					{
						m_fActiveTime = rAction.Time;
					}

					//check if this attack is teh recovery time
					if (m_fRecoveryTime < (rAction.Time + rAction.TimeDelta))
					{
						m_fRecoveryTime = rAction.Time + rAction.TimeDelta;
					}
				}
				else if (m_listActions[i].ActionType == EActionType.Projectile)
				{
					//set this state to an attack state
					m_bIsAttack = true;

					//check if this is the end of the startup 
					CProjectileAction rAction = (CProjectileAction)m_listActions[i];
					if (0.0f == m_fActiveTime)
					{
						m_fActiveTime = rAction.Time;
					}

					//check if this attack is teh recovery time
					if (m_fRecoveryTime < (rAction.Time))
					{
						m_fRecoveryTime = rAction.Time;
					}
				}
			}
		}

		public bool IsAttackActive(GameClock rStateClock)
		{
			Debug.Assert(m_bIsAttack);

			//the attacks are still active if the recovery time hasnt started
			return (rStateClock.CurrentTime < m_fRecoveryTime);
		}

		/// <summary>
		/// Replace all the base object pointers in this dude to point to a replacement object
		/// </summary>
		/// <param name="myBot">the replacement dude</param>
		public void ReplaceOwner(BaseObject myBot)
		{
			//replace in all the state actions
			for (int i = 0; i < m_listActions.Count; i++)
			{
				m_listActions[i].Owner = myBot;
			}
		}

		#region Tool Methods

		/// <summary>
		/// Given an action type, add a blank action to this list of actions
		/// </summary>
		/// <param name="eType">the type of action to add</param>
		/// <param name="rOwner">the owner of this action list</param>
		/// <returns>IBaseAction: reference to the action that was created</returns>
		public IBaseAction AddNewActionFromType(EActionType eType, BaseObject rOwner, IGameDonkey rEngine)
		{
			//get the correct action type
			IBaseAction myAction = null;
			switch (eType)
			{
				case EActionType.AddGarment: { myAction = new AddGarmentAction(rOwner); } break;
				case EActionType.AddVelocity: { myAction = new CAddVelocityAction(rOwner); } break;
				case EActionType.ConstantAcceleration: { myAction = new CConstantAccelerationAction(rOwner); } break;
				case EActionType.ConstantDecceleration: { myAction = new CConstantDeccelerationAction(rOwner); } break;
				case EActionType.CreateAttack: { myAction = new CCreateAttackAction(rOwner); } break;
				case EActionType.CreateBlock: { myAction = new CCreateBlockAction(rOwner); } break;
				case EActionType.CreateThrow: { myAction = new CCreateThrowAction(rOwner); } break;
				case EActionType.Deactivate: { myAction = new CDeactivateAction(rOwner); } break;
				case EActionType.Evade: { myAction = new CEvadeAction(rOwner); } break;
				case EActionType.ParticleEffect: { myAction = new CParticleEffectAction(rOwner, rEngine); } break;
				case EActionType.PlayAnimation: { myAction = new CPlayAnimationAction(rOwner); } break;
				case EActionType.PlaySound: { myAction = new CPlaySoundAction(rOwner, rEngine); } break;
				case EActionType.Projectile: { myAction = new CProjectileAction(rOwner); } break;
				case EActionType.SendStateMessage: { myAction = new CSendStateMessageAction(rOwner); } break;
				case EActionType.SetVelocity: { myAction = new CSetVelocityAction(rOwner); } break;
				case EActionType.Trail: { myAction = new CTrailAction(rOwner); } break;
				case EActionType.BlockState: { myAction = new BlockingStateAction(rOwner); } break;
					
				default: { Debug.Assert(false); } break;
			}

			//save the action
			Debug.Assert(null != myAction);
			m_listActions.Add(myAction);

			//sort the list of actions
			Sort();

			//return the newly created dude
			return myAction;
		}

		/// <summary>
		/// remove an item from the state actions
		/// </summary>
		/// <param name="iActionIndex">index of the item to remove</param>
		public bool RemoveAction(IBaseAction rBaseAction)
		{
			return m_listActions.Remove(rBaseAction);
		}

		public void Sort()
		{
			m_listActions.Sort(new ActionSort());
		}

		#endregion //Tool Methods

		#endregion //Methods

		#region File IO

#if WINDOWS

		public bool ReadSerialized(XmlNode rXMLNode, BaseObject rOwner, IGameDonkey rEngine, StateMachine rStateMachine)
		{
			if ("Item" != rXMLNode.Name)
			{
				Debug.Assert(false);
				return false;
			}

			//should have an attribute Type
			XmlNamedNodeMap mapAttributes = rXMLNode.Attributes;
			for (int i = 0; i < mapAttributes.Count; i++)
			{
				//will only have the name attribute
				string strName = mapAttributes.Item(i).Name;
				string strValue = mapAttributes.Item(i).Value;
				if ("Type" == strName)
				{
					if (strValue != "SPFSettings.StateActionsXML")
					{
						Debug.Assert(false);
						return false;
					}
				}
				else
				{
					Debug.Assert(false);
					return false;
				}
			}

			//Read in child nodes
			if (rXMLNode.HasChildNodes)
			{
				for (XmlNode childNode = rXMLNode.FirstChild;
					null != childNode;
					childNode = childNode.NextSibling)
				{
					//what is in this node?
					string strName = childNode.Name;
					string strValue = childNode.InnerText;

					if (strName == "name")
					{
						m_strStateName = strValue;
					}
					else if (strName == "actions")
					{
						//Read in all the success actions
						if (!IBaseAction.ReadListActions(rOwner, ref m_listActions, childNode, rEngine, rStateMachine))
						{
							Debug.Assert(false);
							return false;
						}
					}
					else
					{
						Debug.Assert(false);
						return false;
					}
				}
			}

			Sort();

			//calculate "active" and "recovery" phases
			CalculateAttackTime();

			return true;
		}

		public void WriteXMLFormat(XmlTextWriter rXMLFile)
		{
			//write out all the state actions!!!

			rXMLFile.WriteStartElement("Item");
			rXMLFile.WriteAttributeString("Type", "SPFSettings.StateActionsXML");

			//write out the state name
			rXMLFile.WriteStartElement("name");
			rXMLFile.WriteString(m_strStateName);
			rXMLFile.WriteEndElement(); //name

			//write out the state actions
			rXMLFile.WriteStartElement("actions");
			for (int i = 0; i < m_listActions .Count; i++)
			{
				m_listActions[i].WriteXMLFormat(rXMLFile);
			}
			rXMLFile.WriteEndElement(); //actions
			rXMLFile.WriteEndElement(); //Item
		}

#endif

		public void ReadSerialized(SPFSettings.StateActionsXML myActions, BaseObject rOwner, IGameDonkey rEngine, ContentManager rXmlContent, StateMachine rStateMachine)
		{
			//grab teh state name
			m_strStateName = myActions.name;

			//verify that the state is in the state machine!
			Debug.Assert(-1 != rStateMachine.GetStateIndexFromText(m_strStateName));

			//set up all the actions
			IBaseAction.ReadListActions(rOwner, myActions.actions, ref m_listActions, rEngine, rXmlContent, rStateMachine);

			//calculate "active" and "recovery" phases
			CalculateAttackTime();
		}

		#endregion //File IO
	}
}