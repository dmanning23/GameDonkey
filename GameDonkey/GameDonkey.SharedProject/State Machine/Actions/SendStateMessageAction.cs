using System;
using System.Diagnostics;
using System.Xml;
using Microsoft.Xna.Framework.Content;

namespace GameDonkey
{
	public class SendStateMessageAction : BaseAction
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
				m_iMessage = StateContainer.GetMessageIndexFromText(m_strMessageName);
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

		private SingleStateContainer StateContainer { get; set; }

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
			Debug.Assert(null != StateContainer);
			Debug.Assert(!AlreadyRun);

			//The message offset is added to this message when it is read in, so dont add anything
			StateContainer.SendStateMessage(m_iMessage);

			//keep running the action until it goes through?
			AlreadyRun = true;
			return true;
		}

		public override bool Compare(BaseAction rInst)
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
		public override bool ReadXml(XmlNode rXMLNode, IGameDonkey rEngine, SingleStateContainer stateContainer, ContentManager content)
		{
			StateContainer = stateContainer;

			#if DEBUG
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
#endif

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

					switch (strName)
					{
						case "type":
						{
							Debug.Assert(strValue == ActionType.ToString());
						}
						break;
						case "time":
						{
							Time = Convert.ToSingle(strValue);
							if (0.0f > Time)
							{
								Debug.Assert(0.0f <= Time);
								return false;
							}
						}
						break;
						case "message":
						{
							MessageName = strValue;
							if (m_iMessage == -1)
							{
								Debug.Assert(false);
								return false;
							}
						}
						break;
						default:
						{
							Debug.Assert(false);
							return false;
						}
					}
				}
			}

			return true;
		}

#if !WINDOWS_UWP
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
#endif

		#endregion //File IO
	}
}