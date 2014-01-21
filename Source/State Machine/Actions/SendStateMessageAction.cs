using StateMachineBuddy;
using System;
using System.Diagnostics;
using System.Xml;

namespace GameDonkey
{
	public class SendStateMessageAction : IBaseAction
	{
		#region Members

		/// <summary>
		/// name of the message this dude sends
		/// </summary>
		protected string m_strMessageName;

		/// <summary>
		/// the message this dude sends
		/// </summary>
		protected int m_iMessage;

		#endregion //Members

		#region Properties

		public string MessageName
		{
			get
			{
				return m_strMessageName;
			}
			set
			{
				m_strMessageName = value;
				m_iMessage = Owner.States.GetMessageIndexFromText(m_strMessageName);
				if (m_iMessage == -1)
				{
					Debug.Assert(false);
				}
			}
		}

		public int MessageIndex
		{
			get { return m_iMessage; }
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Standard constructor
		/// </summary>
		public SendStateMessageAction(BaseObject rOwner) : base(rOwner)
		{
			ActionType = EActionType.SendStateMessage;

			m_strMessageName = "";
			m_iMessage = 0;
		}

		/// <summary>
		/// execute this action (overridden in all child classes)
		/// </summary>
		/// <returns>bool: whether or not to continue running actions after this dude runs</returns>
		public override bool Execute()
		{
			Debug.Assert(null != Owner);
			Debug.Assert(null != Owner.States);
			Debug.Assert(!AlreadyRun);

			//The message offset is added to this message when it is read in, so dont add anything
			Owner.SendStateMessage(m_iMessage);

			//keep running the action until it goes through?
			AlreadyRun = true;
			return true;
		}

		public override bool Compare(IBaseAction rInst)
		{
			SendStateMessageAction myAction = (SendStateMessageAction)rInst;

			Debug.Assert ((ActionType == myAction.ActionType) &&
				(Time == myAction.Time) &&
				(m_strMessageName == myAction.m_strMessageName) &&
				(m_iMessage == myAction.m_iMessage));

			return ((ActionType == myAction.ActionType) &&
				(Time == myAction.Time) &&
				(m_strMessageName == myAction.m_strMessageName) &&
				(m_iMessage == myAction.m_iMessage));
		}

		#endregion //Methods

		#region File IO

		/// <summary>
		/// Read from an xml file
		/// </summary>
		/// <param name="rXMLNode">the xml node to read from</param>
		/// <returns></returns>
		public override bool ReadXml(XmlNode rXMLNode, IGameDonkey rEngine, StateMachine rStateMachine)
		{
			//read in xml action

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
					if (ActionType != StateActionFactory.XMLTypeToType(strValue))
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

					if (strName == "type")
					{
						Debug.Assert(strValue == ActionType.ToString());
					}
					else if (strName == "time")
					{
						Time = Convert.ToSingle(strValue);
						if (0.0f > Time)
						{
							Debug.Assert(0.0f <= Time);
							return false;
						}
					}
					else if (strName == "message")
					{
						MessageName = strValue;
						if (m_iMessage == -1)
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

			return true;
		}

		/// <summary>
		/// overloaded in child classes to write out action specific stuff
		/// </summary>
		/// <param name="rXMLFile"></param>
		protected override void WriteActionXml(XmlTextWriter rXMLFile)
		{
			rXMLFile.WriteStartElement("message");
			rXMLFile.WriteString(m_strMessageName);
			rXMLFile.WriteEndElement();
		}

		/// <summary>
		/// Read from a serialized file
		/// </summary>
		/// <param name="myAction">the xml item to read the action from</param>
		public bool ReadSerialized(SPFSettings.SendStateMessageActionXML myAction, StateMachine rStateMachine)
		{
			Debug.Assert(myAction.type == ActionType.ToString());
			ReadSerializedBase(myAction);

			MessageName = myAction.message;

			return true;
		}

		#endregion //File IO
	}
}