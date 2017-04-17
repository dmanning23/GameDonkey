using GameTimer;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace GameDonkeyLib
{
	/// <summary>
	/// A list of actions to perform while in one state
	/// </summary>
	public class StateActions
	{
		#region Members

		private List<BaseAction> m_listActions;

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

		/// <summary>
		/// name of the state this thing is describing
		/// </summary>
		public string StateName { get; set; }

		/// <summary>
		/// whether or not this is an attack/throw state
		/// </summary>
		public bool IsAttack { get; private set; }

		/// <summary>
		/// List of all the actions to perform when in this state
		/// </summary>
		public List<BaseAction> Actions
		{
			get
			{
				return m_listActions;
			}
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// standard constructor!
		/// </summary>
		public StateActions()
		{
			m_listActions = new List<BaseAction>();
		}

		/// <summary>
		/// The states have changed, go through and set all the actions of the new state to "not run"
		/// </summary>
		public void StateChange()
		{
			for (int i = 0; i < Actions.Count; i++ )
			{
				Actions[i].AlreadyRun = false;
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
			Debug.Assert(null != Actions);

			//loop through all actions, execute the ones between the time slice
			for (int i = 0; i < Actions.Count; i++)
			{
				//first check if the time of this action is expired
				if (Actions[i].Time > fCurTime)
				{
					//this action hasn't been run yet!
					return;
				}

				//check if this action hasn't happened yet
				else if (Actions[i].Time >= fPrevTime)
				{
					//if the action is expired, make sure it has been run anyways
					if (!Actions[i].AlreadyRun)
					{
						if (Actions[i].Execute())
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
			//IBaseAction rLastAction = Actions[Actions.Count - 1];
			//if (rLastAction.AlreadyRun && 
			//    ((rLastAction.ActionType == EActionType.Deactivate) || 
			//    (rLastAction.ActionType == EActionType.SendStateMessage)))
			//{
			//    Debug.Assert(false);
			//}
#endif
		}

		public bool Compare(StateActions rInst)
		{
			if (StateName != rInst.StateName)
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
			if (Actions.Count != rInst.Actions.Count)
			{
				Debug.Assert(false);
				return false;
			}

			//compare all the actions
			for (int i = 0; i < Actions.Count; i++)
			{
				if (!Actions[i].Compare(rInst.Actions[i]))
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
			for (int i = 0; i < Actions.Count; i++)
			{
				if ((Actions[i].ActionType == EActionType.CreateAttack) ||
					(Actions[i].ActionType == EActionType.CreateThrow) ||
					(Actions[i].ActionType == EActionType.CreateHitCircle))
				{
					//set this state to an attack state
					IsAttack = true;

					//check if this is the end of the startup 
					CreateAttackAction rAction = (CreateAttackAction)Actions[i];
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
				else if (Actions[i].ActionType == EActionType.Projectile)
				{
					//set this state to an attack state
					IsAttack = true;

					//check if this is the end of the startup 
					ProjectileAction rAction = (ProjectileAction)Actions[i];
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
			Debug.Assert(IsAttack);

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
			for (int i = 0; i < Actions.Count; i++)
			{
				Actions[i].Owner = myBot;
			}
		}

		public override string ToString()
		{
			return StateName;
		}

		#region Tool Methods

		/// <summary>
		/// Given an action type, add a blank action to this list of actions
		/// </summary>
		/// <param name="eType">the type of action to add</param>
		/// <param name="rOwner">the owner of this action list</param>
		/// <returns>IBaseAction: reference to the action that was created</returns>
		public BaseAction AddNewActionFromType(EActionType eType, BaseObject rOwner, IGameDonkey rEngine)
		{
			//get the correct action type
			BaseAction myAction = StateActionFactory.CreateStateAction(eType, rOwner, rEngine);

			//save the action
			Debug.Assert(null != myAction);
			Actions.Add(myAction);

			//sort the list of actions
			Sort();

			//return the newly created dude
			return myAction;
		}

		/// <summary>
		/// remove an item from the state actions
		/// </summary>
		/// <param name="iActionIndex">index of the item to remove</param>
		public bool RemoveAction(BaseAction rBaseAction)
		{
			return Actions.Remove(rBaseAction);
		}

		public void Sort()
		{
			Actions.Sort(new ActionSort());
		}

		#endregion //Tool Methods

		#endregion //Methods

		#region File IO

		public bool ReadXml(XmlNode xmlNode, BaseObject owner, IGameDonkey engine, SingleStateContainer stateContainer, ContentManager content)
		{
#if DEBUG
			if ("Item" != xmlNode.Name)
			{
				Debug.Assert(false);
				return false;
			}

			//should have an attribute Type
			XmlNamedNodeMap mapAttributes = xmlNode.Attributes;
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
#endif

			//Read in child nodes
			if (xmlNode.HasChildNodes)
			{
				for (XmlNode childNode = xmlNode.FirstChild;
					null != childNode;
					childNode = childNode.NextSibling)
				{
					//what is in this node?
					string strName = childNode.Name;
					string strValue = childNode.InnerText;

					if (strName == "name")
					{
						StateName = strValue;
					}
					else if (strName == "actions")
					{
						//Read in all the success actions
						if (!BaseAction.ReadXmlListActions(owner, ref m_listActions, childNode, engine, stateContainer, content))
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

#if !WINDOWS_UWP
		public void WriteXml(XmlTextWriter rXMLFile)
		{
			//write out all the state actions!!!

			rXMLFile.WriteStartElement("Item");
			rXMLFile.WriteAttributeString("Type", "SPFSettings.StateActionsXML");

			//write out the state name
			rXMLFile.WriteStartElement("name");
			rXMLFile.WriteString(StateName);
			rXMLFile.WriteEndElement(); //name

			//write out the state actions
			rXMLFile.WriteStartElement("actions");
			for (int i = 0; i < Actions .Count; i++)
			{
				Actions[i].WriteXml(rXMLFile);
			}
			rXMLFile.WriteEndElement(); //actions
			rXMLFile.WriteEndElement(); //Item
		}
#endif

		#endregion //File IO
	}
}